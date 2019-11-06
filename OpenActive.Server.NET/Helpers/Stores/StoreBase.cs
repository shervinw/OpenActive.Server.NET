using OpenActive.DatasetSite.NET;
using OpenActive.NET;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.Server.NET
{
    public interface IOpenBookingStore
    {
        void SetConfiguration(OpportunityTypeConfiguration OpportunityTypeConfiguration, BookingEngineSettings settings, IBookablePairIdTemplate template, Uri openDataFeedBaseUrl);
        /*
        OrderItem GetOrderItem(IBookableIdComponents opportunityOfferId, ISingleIdTemplate sellerId);
        */


        void CreateTestDataItem(OpportunityType opportunityType, Event @event);
        void DeleteTestDataItem(OpportunityType opportunityType, string name);


    }


    //TODO: Remove duplication between this and RpdeBase if possible as they are using the same pattern?
    public abstract class OpenBookingStore<TComponents> : IOpenBookingStore where TComponents : IBookableIdComponents, new()
    {
        public Uri JsonLdIdBaseUrl { get; private set; }
        private BookablePairIdTemplate<TComponents> IdTemplate { get; set; }
        private OpportunityTypeConfiguration OpportunityTypeConfiguration { get; set; }
        private BookingEngineSettings BookingEngineSettings { get; set; }

        public void SetConfiguration(OpportunityTypeConfiguration opportunityTypeConfiguration, BookingEngineSettings settings, IBookablePairIdTemplate template, Uri openDataFeedBaseUrl)
        {
            if (!(template.GetType() == typeof(BookablePairIdTemplate<TComponents>) || template.GetType() == typeof(BookablePairIdTemplateWithOfferInheritance<TComponents>)))
            {
                throw new NotSupportedException($"{template.GetType().ToString()} does not match {typeof(BookablePairIdTemplate<TComponents>).ToString()}. All types of IBookableIdComponents (T) used for BookablePairIdTemplate<T> assigned to feeds via settings.IdConfiguration must match those used for RPDEFeedGenerator<T> in settings.OpenDataFeeds.");
            }

            SetConfiguration(opportunityTypeConfiguration, settings, (BookablePairIdTemplate<TComponents>)template, openDataFeedBaseUrl);
        }

        internal void SetConfiguration(OpportunityTypeConfiguration opportunityTypeConfiguration, BookingEngineSettings settings, BookablePairIdTemplate<TComponents> template, Uri openDataFeedBaseUrl)
        {
            this.OpportunityTypeConfiguration = opportunityTypeConfiguration;
            this.BookingEngineSettings = settings;
            this.IdTemplate = template;

            this.JsonLdIdBaseUrl = settings.JsonLdIdBaseUrl;
        }

        protected Uri RenderOpportunityId(OpportunityType opportunityType, TComponents components)
        {
            return IdTemplate.RenderOpportunityId(opportunityType, components);
        }

        protected Uri RenderOfferId(OpportunityType opportunityType, TComponents components)
        {
            return IdTemplate.RenderOfferId(opportunityType, components);
        }

        public abstract OrderItem GetOrderItem(TComponents opportunityOfferId, SellerIdComponents sellerId);
        public abstract void CreateTestDataItem(OpportunityType opportunityType, Event @event);
        public abstract void DeleteTestDataItem(OpportunityType opportunityType, string name);
    }
}
