using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UriTemplate.Core;

namespace OpenActive.Server.NET
{
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

    public class BookablePairIdTemplate<T> : IdTemplate<T> where T : new()
    {
        public BookablePairIdTemplate(string opportunityUriTemplate, string offerUriTemplate) : base(opportunityUriTemplate, offerUriTemplate)
        {
        }

        public T GetIdComponents(Uri opportunityId, Uri offerId)
        {
            return base.GetIdComponents(opportunityId, offerId);
        }

        public Uri RenderOpportunityId(T components)
        {
            return RenderId(0, components);
        }
        public Uri RenderOfferId(T components)
        {
            return RenderId(1, components);
        }
    }

    public class SingleIdTemplate<T> : IdTemplate<T> where T : new()
    {
        public SingleIdTemplate(string uriTemplate) : base(uriTemplate)
        {
        }

        public T GetIdComponents(Uri id)
        {
            return base.GetIdComponents(id);
        }

        public Uri RenderId(T components)
        {
            return RenderId(0, components);
        }

    }

    /// <summary>
    /// Id transforms provide strongly typed
    /// </summary>
    public abstract class IdTemplate<T> where T : new()
    {
        private List<UriTemplate.Core.UriTemplate> uriTemplates;

        //IdTemplate<ScheduledSessionIdComponents>("<scheduled_session_url", "offer_url")

        protected IdTemplate(params string[] uriTemplate)
        {
            uriTemplates = uriTemplate.Select(t => new UriTemplate.Core.UriTemplate(t)).ToList();
        }

        protected T GetIdComponents(params Uri[] ids)
        {
            if (ids.Length != uriTemplates.Count)
                throw new ArgumentException("Supplied ids must match number of UriTemplates in order");

            var matches = ids.Zip(uriTemplates, (id, uriTemplate) => uriTemplate.Match(id));

            var components = new T();
            var componentsType = typeof(T);

            foreach (UriTemplateMatch match in matches)
            {
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
                    } else
                    {
                        throw new ArgumentException("Only types long? and string are supported within the component class used for IdTemplate.");
                    }
                }
            }

            return components;
        }

        protected Uri RenderId(int index, T components)
        {
            var componentDictionary = components.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                     .ToDictionary(prop => prop.Name, prop => prop.GetValue(components, null));

            return uriTemplates[index].BindByName(componentDictionary);
        }
    }

}
