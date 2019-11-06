using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UriTemplate.Core;
using OpenActive.NET;
using OpenActive.DatasetSite.NET;
using System.Collections;

namespace OpenActive.Server.NET
{
    public interface IBookableIdComponents {
        Uri BaseUrl { get; set; }
        OpportunityType? OpportunityType { get; set; }
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
        //OpportunityIdConfiguration OpportunityIdConfiguration { get;  }
        //OpportunityIdConfiguration? ParentIdConfiguration { get; }
        //OpportunityIdConfiguration? GrandparentIdConfiguration { get; }
        List<OpportunityIdConfiguration> IdConfigurations { get; }

        IBookableIdComponents GetOpportunityReference(Uri opportunityId, Uri offerId);

        //Uri RenderOfferId(OpportunityType opportunityType, IBookableIdComponents components);
        //Uri RenderOpportunityId(OpportunityType opportunityType, IBookableIdComponents components);

    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "<Pending>")]
    public class BookablePairIdTemplateWithOfferInheritance<T> : BookablePairIdTemplate<T> where T : IBookableIdComponents, new()
    {

        public BookablePairIdTemplateWithOfferInheritance(
            OpportunityIdConfiguration opportunityIdConfiguration,
            OpportunityIdConfiguration? parentIdConfiguration = null,
            OpportunityIdConfiguration? grandparentIdConfiguration = null)
            : base(false, opportunityIdConfiguration, parentIdConfiguration, grandparentIdConfiguration)
        {
            // ScheduledSession with SessionSeries is the only opportunity type that allows Offer inheritance within Modelling Specification 2.0
            // Therefore the check below ensures that this class is only used in accordance with the spec
            if (opportunityIdConfiguration.OpportunityType != OpportunityType.ScheduledSession)
            {
                throw new NotSupportedException($"{nameof(BookablePairIdTemplateWithOfferInheritance<T>)} used with unsupported {nameof(OpportunityType)} pair. ScheduledSession (from SessionSeries) is the only opportunity type that allows Offer inheritance within Modelling Specification 2.0. Please use {nameof(BookablePairIdTemplate<T>)}.");
            }

        }

        /// <summary>
        /// This is used by the booking engine to resolve an OrderItem to its components, using only opportunityId and Uri offerId
        /// </summary>
        /// <param name="opportunityId"></param>
        /// <param name="offerId"></param>
        /// <returns>Null if either ID does not match the template for the Opportunity, with its own Offer or the Offer of its parent</returns>
        public override IBookableIdComponents GetOpportunityReference(Uri opportunityId, Uri offerId)
        {
            // Require both opportunityId and offerId to not be null
            if (opportunityId == null) throw new ArgumentNullException(nameof(opportunityId));
            if (offerId == null) throw new ArgumentNullException(nameof(offerId));

            // As inheritance is in use, the Offer must be resolved against either: Opportunity with Offer; or Opportunity and _parent_ Offer
            // Note in OpenActive Modelling Specification 2.0 this behaviour is only applicable to SessionSeries and ScheduledSession
            // Note the grandparent (e.g. EventSeries) is never bookable

            // TODO: Make this check for this.OpportunityIdConfiguration.Bookable == true

            return GetIdComponentsWithOpportunityType(this.OpportunityIdConfiguration.OpportunityType, opportunityId, offerId, null, null)
            ?? GetIdComponentsWithOpportunityType(this.ParentIdConfiguration?.OpportunityType, opportunityId, null, null, offerId);

        }
    }

    public struct OpportunityIdConfiguration
    {
        public OpportunityType OpportunityType { get; set; }
        public OpportunityType AssignedFeed { get; set; }
        public string OpportunityIdTemplate { get; set; }
        public string OfferIdTemplate { get; set; }
        public bool Bookable { get; set; } 
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "<Pending>")]
    public class BookablePairIdTemplate<T> : IdTemplate<T>, IBookablePairIdTemplate where T : IBookableIdComponents, new()
    {
        public BookablePairIdTemplate(
            OpportunityIdConfiguration opportunityIdConfiguration,
            OpportunityIdConfiguration? parentIdConfiguration = null,
            OpportunityIdConfiguration? grandparentIdConfiguration = null)
            : this(false, opportunityIdConfiguration, parentIdConfiguration, grandparentIdConfiguration)
        {
            // ScheduledSession with SessionSeries is the only opportunity type that allows Offer inheritance within Modelling Specification 2.0
            // Therefore the check below ensures that this class is only used in accordance with the spec
            if (opportunityIdConfiguration.OpportunityType == OpportunityType.ScheduledSession)
            {
                throw new NotSupportedException($"{nameof(BookablePairIdTemplate<T>)} used with unsupported {nameof(OpportunityType)} pair. ScheduledSession with SessionSeries are the only opportunity types that allows Offer inheritance within Modelling Specification 2.0. Please use {nameof(BookablePairIdTemplateWithOfferInheritance<T>)}.");
            }
        }

        protected BookablePairIdTemplate(bool overrideSessionSeriesCheck,
            OpportunityIdConfiguration opportunityIdConfiguration,
            OpportunityIdConfiguration? parentIdConfiguration,
            OpportunityIdConfiguration? grandparentIdConfiguration)
            : base(opportunityIdConfiguration.OpportunityIdTemplate, opportunityIdConfiguration.OfferIdTemplate,
                  parentIdConfiguration?.OpportunityIdTemplate, parentIdConfiguration?.OfferIdTemplate,
                  grandparentIdConfiguration?.OpportunityIdTemplate, grandparentIdConfiguration?.OfferIdTemplate)
        {


            // SH-TODO: Add more code here to validate the combinations of child/parent/grandparent OpportunityType based on the Parent relationship
            // in OpportunityTypes.Configuration. Throw an error if the user attempts to create something invalid.
            // Also check that anything that's set as Bookable = true, at least ahs Bookable = true in OpportunityTypes.Configuration

            this.OpportunityIdConfiguration = opportunityIdConfiguration;
            this.ParentIdConfiguration = parentIdConfiguration;
            this.GrandparentIdConfiguration = grandparentIdConfiguration;
            
            // Create list to simplify access
            var list = new List<OpportunityIdConfiguration> { this.OpportunityIdConfiguration };
            if (this.ParentIdConfiguration.HasValue) list.Add(this.ParentIdConfiguration.Value);
            if (this.GrandparentIdConfiguration.HasValue) list.Add(this.GrandparentIdConfiguration.Value);
            this.IdConfigurations = list;
        }


        protected OpportunityIdConfiguration OpportunityIdConfiguration { get; }
        protected OpportunityIdConfiguration? ParentIdConfiguration { get;}
        protected OpportunityIdConfiguration? GrandparentIdConfiguration { get; }
        public List<OpportunityIdConfiguration> IdConfigurations { get; }


        /// <summary>
        /// This is used by the booking engine to resolve an OrderItem to its components, using only opportunityId and Uri offerId
        /// </summary>
        /// <param name="opportunityId"></param>
        /// <param name="offerId"></param>
        /// <returns>Null if either ID does not match the template</returns>
        public virtual IBookableIdComponents GetOpportunityReference(Uri opportunityId, Uri offerId)
        {
            // Design decision: this is virtual as the params are derived directly from the Open Booking API
            // and so are unlikely to change. Noting risks around maintaining a virtual method in a public library.

            // Require both opportunityId and offerId to not be null
            if (opportunityId == null) throw new ArgumentNullException(nameof(opportunityId));
            if (offerId == null) throw new ArgumentNullException(nameof(offerId));

            // Without inheritance, the Offer must be resolved against either: Opportunity with Offer; or _parent_ Opportunity and parent Offer
            // Note that if any URL templates to be used for one of the checks below are null, the result for that check will be null
            // TODO: Document that this library purposely does not support Event with subEvent of Event, as this is not recommended in Model 2.0
            // and has many edge cases
            // Note the grandparent is never bookable

            return GetIdComponentsWithOpportunityType(this.OpportunityIdConfiguration.OpportunityType, opportunityId, offerId, null, null)
                ?? GetIdComponentsWithOpportunityType(this.ParentIdConfiguration?.OpportunityType, null, null, opportunityId, offerId);
        }

        protected IBookableIdComponents GetIdComponentsWithOpportunityType(OpportunityType? opportunityType, params Uri[] ids)
        {
            var components = base.GetIdComponents((nameof(GetOpportunityReference)), ids);
            if (components != null)
            {
                if (!opportunityType.HasValue) throw new ArgumentNullException("Unexpected match with invalid OpportunityIdConfiguration.");
                components.OpportunityType = opportunityType.Value;
                return components;
            }
            else
            {
                return null;
            }
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
         * Note: this is not provided as an option as it should never need to be used in a sane implementation of Open Booking API
        public T GetIdComponents(Uri opportunityId, Uri offerId, Uri parentOpportunityId, Uri parentOfferId)
        {
            return base.GetIdComponents(nameof(GetIdComponents), opportunityId, offerId, parentOpportunityId, parentOfferId);
        }*/

        // TODO: Fix strings below to include accurate error messages

        public Uri RenderOpportunityId(OpportunityType opportunityType, T components)
        {
            if (opportunityType == OpportunityIdConfiguration.OpportunityType)
                return RenderId(0, components, nameof(RenderOpportunityId), "opportunityUriTemplate");
            else if (opportunityType == ParentIdConfiguration?.OpportunityType)
                return RenderId(2, components, nameof(RenderOpportunityId), "parentOpportunityUriTemplate");
            else if (opportunityType == GrandparentIdConfiguration?.OpportunityType)
                return RenderId(4, components, nameof(RenderOpportunityId), "parentOpportunityUriTemplate");
            else
                throw new ArgumentOutOfRangeException(nameof(opportunityType), "OpportunityType was not found within this template");
        }

        public Uri RenderOfferId(OpportunityType opportunityType, T components)
        {
            if (opportunityType == OpportunityIdConfiguration.OpportunityType)
                return RenderId(1, components, nameof(RenderOfferId), "offerUriTemplate");
            else if (opportunityType == ParentIdConfiguration?.OpportunityType)
                return RenderId(3, components, nameof(RenderOfferId), "parentOfferUriTemplate");
            else if (opportunityType == GrandparentIdConfiguration?.OpportunityType)
                return RenderId(5, components, nameof(RenderOfferId), "parentOfferUriTemplate");
            else
                throw new ArgumentOutOfRangeException(nameof(opportunityType), "OpportunityType was not found within this template");
        }

        /*
        public Uri RenderOpportunityId(OpportunityType opportunityType, IBookableIdComponents components)
        {
            return RenderOpportunityId(opportunityType, (T)components);
        }

        public Uri RenderOfferId(OpportunityType opportunityType, IBookableIdComponents components)
        {
            return RenderOfferId(opportunityType, (T)components);
        }
        */
        
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
            uriTemplates = uriTemplate.Select(t => t == null ? null : new UriTemplate.Core.UriTemplate(t)).ToList();
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

            // TODO: Create detailed tests around this and ensure that if components that match are zero length or whitespace,
            // it behaves the same as if no match has occurred

            for (var index = 0; index < ids.Length; index++)
            {
                // Ignore an id where it is supplied as null
                if (ids[index] == null) continue;

                // If one of the urlTemplates in scope of the match is null, return null
                if (uriTemplates[index] == null) return default(T);

                var match = uriTemplates[index].Match(ids[index]);

                // If ID does match template, return null
                if (match.Bindings.Count == 0) return default(T);

                // Set matching components in supplied POCO based on property name
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
