using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UriTemplate.Core;
using OpenActive.NET;

namespace OpenActive.Server.NET
{
    public interface IBookableIdComponents {
        Uri BaseUrl { get; set; }
    }

    public class OrderId
    {
        public Uri BaseUrl { get; set; }
        public string uuid { get; set; }
    }

    public class BookableOpportunityAndOfferMismatchException : Exception
    {
        public BookableOpportunityAndOfferMismatchException()
        {
        }

        public BookableOpportunityAndOfferMismatchException(string message)
            : base(message)
        {
        }

        public BookableOpportunityAndOfferMismatchException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public interface IBookablePairIdTemplate
    {
        IBookableIdComponents GetOpportunityReference(Uri opportunityId, Uri offerId);
        Uri RenderOfferId(IBookableIdComponents components);
        Uri RenderOpportunityId(IBookableIdComponents components);
        Uri RenderParentOfferId(IBookableIdComponents components);
        Uri RenderParentOpportunityId(IBookableIdComponents components);
    }

    public class BookablePairIdTemplateWithOfferInheritance<T> : BookablePairIdTemplate<T> where T : IBookableIdComponents, new()
    {
        public BookablePairIdTemplateWithOfferInheritance(string opportunityUriTemplate, string offerUriTemplate, string parentOpportunityUriTemplate, string parentOfferUriTemplate)
        : base(opportunityUriTemplate, offerUriTemplate, parentOpportunityUriTemplate, parentOfferUriTemplate)
        {
        }

        /// <summary>
        /// This is used by the booking engine to resolve an OrderItem to its components, using only opportunityId and Uri offerId
        /// </summary>
        /// <param name="opportunityId"></param>
        /// <param name="offerId"></param>
        /// <returns>Null if either ID does not match the template for the Opportunity, with its own Offer or the Offer of its parent</returns>
        public new IBookableIdComponents GetOpportunityReference(Uri opportunityId, Uri offerId)
        {
            // Require both opportunityId and offerId to not be null
            if (opportunityId == null) throw new ArgumentNullException(nameof(opportunityId));
            if (offerId == null) throw new ArgumentNullException(nameof(offerId));

            // As inheritance is in use, the Offer must be resolved against either: Opportunity with Offer; or Opportunity and parent Offer
            // Note in OpenActive Modelling Specification 2.0 this behaviour is only applicable to SessionSeries and ScheduledSession
            return (IBookableIdComponents)base.GetIdComponents(nameof(GetIdComponents), opportunityId, offerId, null, null)
                ?? (IBookableIdComponents)base.GetIdComponents(nameof(GetIdComponents), opportunityId, null, null, offerId);
        }
    }

    public class BookablePairIdTemplate<T> : IdTemplate<T>, IBookablePairIdTemplate where T : IBookableIdComponents, new()
    {
        public BookablePairIdTemplate(string opportunityUriTemplate, string offerUriTemplate) : base(opportunityUriTemplate, offerUriTemplate)
        {
        }
        public BookablePairIdTemplate(string opportunityUriTemplate, string offerUriTemplate, string parentOpportunityUriTemplate)
            : base(opportunityUriTemplate, offerUriTemplate, parentOpportunityUriTemplate)
        {
        }
        protected BookablePairIdTemplate(string opportunityUriTemplate, string offerUriTemplate, string parentOpportunityUriTemplate, string parentOfferUriTemplate)
: base(opportunityUriTemplate, offerUriTemplate, parentOpportunityUriTemplate, parentOfferUriTemplate)
        {
        }

        /// <summary>
        /// This is used by the booking engine to resolve an OrderItem to its components, using only opportunityId and Uri offerId
        /// </summary>
        /// <param name="opportunityId"></param>
        /// <param name="offerId"></param>
        /// <returns>Null if either ID does not match the template</returns>
        public IBookableIdComponents GetOpportunityReference(Uri opportunityId, Uri offerId)
        {
            // Require both opportunityId and offerId to not be null
            if (opportunityId == null) throw new ArgumentNullException(nameof(opportunityId));
            if (offerId == null) throw new ArgumentNullException(nameof(offerId));
            return base.GetIdComponents(nameof(GetOpportunityReference), opportunityId, offerId);
        }

        public T GetIdComponents(Uri opportunityId, Uri offerId)
        {
            return base.GetIdComponents(nameof(GetIdComponents), opportunityId, offerId);
        }
        public T GetIdComponents(Uri opportunityId, Uri offerId, Uri parentOpportunityId)
        {
            return base.GetIdComponents(nameof(GetIdComponents), opportunityId, offerId, parentOpportunityId);
        }
        /*
         * Note: this is not provided as an option as it should never need to be used in a sane implementation
        public T GetIdComponents(Uri opportunityId, Uri offerId, Uri parentOpportunityId, Uri parentOfferId)
        {
            return base.GetIdComponents(nameof(GetIdComponents), opportunityId, offerId, parentOpportunityId, parentOfferId);
        }*/

        public Uri RenderOpportunityId(T components)
        {
            return RenderId(0, components, nameof(RenderOpportunityId), "opportunityUriTemplate");
        }
        public Uri RenderOfferId(T components)
        {
            return RenderId(1, components, nameof(RenderOfferId), "offerUriTemplate");
        }
        public Uri RenderParentOpportunityId(T components)
        {
            return RenderId(2, components, nameof(RenderParentOpportunityId), "parentOpportunityUriTemplate");
        }
        public Uri RenderParentOfferId(T components)
        {
            return RenderId(3, components, nameof(RenderParentOfferId), "parentOfferUriTemplate");
        }


        public Uri RenderOpportunityId(IBookableIdComponents components)
        {
            return RenderId(0, (T)components, nameof(RenderOpportunityId), "opportunityUriTemplate");
        }

        public Uri RenderOfferId(IBookableIdComponents components)
        {
            return RenderId(1, (T)components, nameof(RenderOfferId), "offerUriTemplate");
        }
        public Uri RenderParentOpportunityId(IBookableIdComponents components)
        {
            return RenderId(2, (T)components, nameof(RenderParentOpportunityId), "parentOpportunityUriTemplate");
        }
        public Uri RenderParentOfferId(IBookableIdComponents components)
        {
            return RenderId(3, (T)components, nameof(RenderParentOfferId), "parentOfferUriTemplate");
        }
    }

    public class SingleIdTemplate<T> : IdTemplate<T> where T : new()
    {
        public SingleIdTemplate(string uriTemplate) : base(uriTemplate)
        {
            if (uriTemplate == null) throw new ArgumentNullException(nameof(uriTemplate));
        }

        public T GetIdComponents(Uri id)
        {
            return base.GetIdComponents(nameof(GetIdComponents) , id);
        }

        public Uri RenderId(T components)
        {
            return RenderId(0, components, nameof(RenderId),  "uriTemplate");
        }

    }

    /// <summary>
    /// Id transforms provide strongly typed
    /// </summary>
    public abstract class IdTemplate<T> where T : new()
    {
        private List<UriTemplate.Core.UriTemplate> uriTemplates;

        protected IdTemplate(params string[] uriTemplate)
        {
            uriTemplates = uriTemplate.Select(t => new UriTemplate.Core.UriTemplate(t)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="matchRequired">Defaults to bool[] { true, true, ... } if not set</param>
        /// <param name="ids"></param>
        /// <returns></returns>
        protected T GetIdComponents(string method, params Uri[] ids)
        {
            if (ids.Length > uriTemplates.Count)
                throw new ArgumentException("{method} must have a number of supplied id parameters that are at least covered by the number of templates supplied when using the constructor for this class. Pass null for these ids if not known.");

            var components = new T();
            var componentsType = typeof(T);

            for (var index = 0; index < ids.Length; index++)
            {
                // Ignore an id where it is supplied as null
                if (ids[index] == null) continue;

                var match = uriTemplates[index].Match(ids[index]);

                // If ID does match template, return null
                if (match.Bindings.Count == 0)
                {
                    return default(T);
                }

                foreach (var binding in match.Bindings)
                {
                    if (componentsType.GetProperty(binding.Key) == null) throw new ArgumentException("Supplied UriTemplates must match supplied component type properties");

                    if (componentsType.GetProperty(binding.Key).PropertyType == typeof(long?))
                    {
                        if (long.TryParse(binding.Value.Value as string, out long newValue))
                        {
                            var existingValue = componentsType.GetProperty(binding.Key).GetValue(components) as long?;
                            if (existingValue != newValue && existingValue != null)
                            {
                                throw new BookableOpportunityAndOfferMismatchException($"Supplied Ids do not match on component '{binding.Value.Key}'");
                            }
                            componentsType.GetProperty(binding.Key).SetValue(components, newValue);
                        }
                        else
                        {
                            throw new ArgumentException($"An integer in the template for binding {binding.Key} failed to parse.");
                        }
                    }
                    else if (componentsType.GetProperty(binding.Key).PropertyType == typeof(string))
                    {
                        var newValue = binding.Value.Value as string;
                        var existingValue = componentsType.GetProperty(binding.Key).GetValue(components) as string;
                        if (existingValue != newValue && existingValue != null)
                        {
                            throw new BookableOpportunityAndOfferMismatchException($"Supplied Ids do not match on component '{binding.Value.Key}'");
                        }
                        componentsType.GetProperty(binding.Key).SetValue(components, newValue);
                    }
                    else if (componentsType.GetProperty(binding.Key).PropertyType == typeof(Uri))
                    {
                        var newValue = (binding.Value.Value as string).ParseUrlOrNull();
                        var existingValue = componentsType.GetProperty(binding.Key).GetValue(components) as Uri;
                        if (existingValue != newValue && existingValue != null)
                        {
                            throw new BookableOpportunityAndOfferMismatchException($"Supplied Ids do not match on component '{binding.Value.Key}'");
                        }
                        componentsType.GetProperty(binding.Key).SetValue(components, newValue);
                    }
                    else
                    {
                        throw new ArgumentException("Only types long?, Uri and string are supported within the component class used for IdTemplate.");
                    }
                }
            }

            return components;
        }

        protected Uri RenderId(int index, T components, string method, string param)
        {
            if (uriTemplates.ElementAtOrDefault(index) == null)
            {
                throw new NotSupportedException($"{method} is not available as {param} was not specified when using the constructor for this class.");
            }

            var componentDictionary = components.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                     .ToDictionary(prop => prop.Name, prop => prop.GetValue(components, null));

            return uriTemplates[index].BindByName(componentDictionary);
        }
    }

}
