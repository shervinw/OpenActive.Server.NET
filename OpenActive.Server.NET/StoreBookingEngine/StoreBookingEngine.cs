﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenActive.DatasetSite.NET;
using OpenActive.NET;
using OpenActive.NET.Rpde.Version1;
using OpenActive.Server.NET.OpenBookingHelper;
using OpenActive.Server.NET.CustomBooking;

namespace OpenActive.Server.NET.StoreBooking
{
    public interface IOrderItemContext
    {
        int Index { get; set; }
        IBookableIdComponents RequestBookableOpportunityOfferId { get; set; }
        OrderIdComponents ResponseOrderItemId { get; }
        OrderItem RequestOrderItem { get; set; }
        OrderItem ResponseOrderItem { get; }
    }

    public class OrderItemContext<TComponents> : IOrderItemContext where TComponents : IBookableIdComponents
    {
        public int Index { get; set; }
        public TComponents RequestBookableOpportunityOfferId { get; set; }
        IBookableIdComponents IOrderItemContext.RequestBookableOpportunityOfferId { get => this.RequestBookableOpportunityOfferId; set => this.RequestBookableOpportunityOfferId = (TComponents)value; }
        public OrderIdComponents ResponseOrderItemId { get; private set; }
        public OrderItem RequestOrderItem { get; set; }
        public OrderItem ResponseOrderItem { get; private set; }

        public void AddError(OpenBookingError openBookingError)
        {
            if (ResponseOrderItem == null) throw new NotSupportedException("AddError cannot be used before SetResponseOrderItem.");
            if (ResponseOrderItem.Error == null) ResponseOrderItem.Error = new List<OpenBookingError>();
            ResponseOrderItem.Error.Add(openBookingError);
        }

        public void SetOrderItemId(StoreBookingFlowContext flowContext, string orderItemId)
        {
            SetOrderItemId(flowContext, null, orderItemId);
        }

        public void SetOrderItemId(StoreBookingFlowContext flowContext, long orderItemId)
        {
            SetOrderItemId(flowContext, orderItemId, null);
        }

        private void SetOrderItemId(StoreBookingFlowContext flowContext, long? orderItemIdLong, string orderItemIdString)
        {
            if (flowContext == null) throw new ArgumentNullException(nameof(flowContext));
            if (ResponseOrderItem == null) throw new NotSupportedException("SetOrderItemId cannot be used before SetResponseOrderItem.");
            ResponseOrderItemId = new OrderIdComponents
            {
                uuid = flowContext.OrderId.uuid,
                OrderType = flowContext.OrderId.OrderType,
                OrderItemIdString = orderItemIdString,
                OrderItemIdLong = orderItemIdLong
            };
            ResponseOrderItem.Id = flowContext.OrderIdTemplate.RenderOrderItemId(ResponseOrderItemId);
        }

        public void SetResponseOrderItemAsSkeleton()
        {
            ResponseOrderItem = new OrderItem
            {
                AcceptedOffer = new Offer
                {
                    Id = RequestOrderItem?.AcceptedOffer?.Id
                },
                OrderedItem = OrderCalculations.RenderOpportunityWithOnlyId(RequestOrderItem?.OrderedItem?.Type, RequestOrderItem?.OrderedItem?.Id)
            };
        }

        public void SetResponseOrderItem(OrderItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item?.OrderedItem?.Id != RequestOrderItem?.OrderedItem?.Id)
            {
                throw new ArgumentException("The Opportunity ID within the response OrderItem must match the request OrderItem");
            }
            if (item?.AcceptedOffer?.Id != RequestOrderItem?.AcceptedOffer?.Id)
            {
                throw new ArgumentException("The Offer ID within the response OrderItem must match the request OrderItem");
            }
            ResponseOrderItem = item;
        }

        public void ValidateAttendeeDetails()
        {
            if (ResponseOrderItem == null) throw new NotSupportedException("ValidateAttendeeDetails cannot be used before SetResponseOrderItem.");
            OrderCalculations.ValidateAttendeeDetails(RequestOrderItem, ResponseOrderItem);
        }
    }

    /// <summary>
    /// The StoreBookingEngine provides a more opinionated implementation of the Open Booking API on top of AbstractBookingEngine.
    /// This is designed to be quick to implement, but may not fit the needs of more complex systems.
    /// 
    /// It is not designed to be subclassed (it could be sealed?), but instead the implementer is encouraged
    /// to implement and provide an IOpenBookingStore on instantiation. 
    /// </summary>
    public class StoreBookingEngine : CustomBookingEngine
    {
        /// <summary>
        /// Simple contructor
        /// </summary>
        /// <param name="settings">Settings are used exclusively by the AbstractBookingEngine</param>
        /// <param name="store">Store used exclusively by the StoreBookingEngine</param>
        public StoreBookingEngine(BookingEngineSettings settings, DatasetSiteGeneratorSettings datasetSettings, StoreBookingEngineSettings storeBookingEngineSettings) : base(settings, datasetSettings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (datasetSettings == null) throw new ArgumentNullException(nameof(datasetSettings));
            if (storeBookingEngineSettings == null) throw new ArgumentNullException(nameof(storeBookingEngineSettings));

            this.stores = storeBookingEngineSettings.OpportunityStoreRouting.Keys.ToList();
            this.storeBookingEngineSettings = storeBookingEngineSettings;

            // TODO: Add test to ensure there are not two or more at FirstOrDefault step, in case of configuration error 
            this.storeRouting = storeBookingEngineSettings.OpportunityStoreRouting.Select(t => t.Value.Select(y => new
            {
                store = t.Key,
                opportunityType = y
            })).SelectMany(x => x.ToList()).GroupBy(g => g.opportunityType).ToDictionary(k => k.Key, v => v.Select(a => a.store).SingleOrDefault());

            // Setup each store with the relevant settings, including the relevant IdTemplate inferred from the config
            var storeConfig = storeBookingEngineSettings.OpportunityStoreRouting
                .ToDictionary(k => k.Key, v => v.Value.Select(y => base.OpportunityTemplateLookup[y]).Distinct().Single());
            foreach (var store in storeConfig)
            {
                store.Key.SetConfiguration(store.Value, settings.SellerIdTemplate);
            }
        }

        private readonly List<IOpportunityStore> stores;
        private readonly Dictionary<OpportunityType, IOpportunityStore> storeRouting;
        private readonly StoreBookingEngineSettings storeBookingEngineSettings;
        
        protected override Event CreateTestDataItem(OpportunityType opportunityType, Event @event)
        {
            if (!storeRouting.ContainsKey(opportunityType))
                throw new OpenBookingException(new OpenBookingError(), "Specified opportunity type is not configured as bookable in the StoreBookingEngine constructor.");

            //TODO: This forces the cast into the Store. Perhaps best to move the cast here to simplify the store?
            return storeRouting[opportunityType].CreateTestDataItemEvent(opportunityType, @event);
        }

        protected override void DeleteTestDataItem(OpportunityType opportunityType, Uri id)
        {
            if (!storeRouting.ContainsKey(opportunityType))
                throw new OpenBookingException(new OpenBookingError(), "Specified opportunity type is not configured as bookable in the StoreBookingEngine constructor.");

            storeRouting[opportunityType].DeleteTestDataItemEvent(opportunityType, id);
        }



        public override void ProcessCustomerCancellation(OrderIdComponents orderId, SellerIdComponents sellerId, OrderIdTemplate orderIdTemplate, List<OrderIdComponents> orderItemIds)
        {
            if (!storeBookingEngineSettings.OrderStore.CustomerCancelOrderItems(orderId, sellerId, orderIdTemplate, orderItemIds))
            {
                throw new OpenBookingException(new NotFoundError(), "Order not found");
            }
        }

        protected override void ProcessOrderDeletion(OrderIdComponents orderId, SellerIdComponents sellerId)
        {
            storeBookingEngineSettings.OrderStore.DeleteOrder(orderId, sellerId);
        }

        protected override void ProcessOrderQuoteDeletion(OrderIdComponents orderId, SellerIdComponents sellerId)
        {
            storeBookingEngineSettings.OrderStore.DeleteLease(orderId, sellerId);
        }


        public override TOrder ProcessFlowRequest<TOrder>(BookingFlowContext request, TOrder order)
        {
            StoreBookingFlowContext context = new StoreBookingFlowContext(request);

            // Throw error on incomplete customer details if C2 or B
            if (context.Stage != FlowStage.C1 && (order.Customer == null || string.IsNullOrWhiteSpace(order.Customer.Email)))
            {
                throw new OpenBookingException(new IncompleteCustomerDetailsError());
            }

            // Reflect back only those customer fields that are supported
            switch (order.Customer)
            {
                case Organization organization:
                    context.Customer = storeBookingEngineSettings.CustomerOrganizationSupportedFields(organization);
                    break;

                case Person person:
                    context.Customer = storeBookingEngineSettings.CustomerPersonSupportedFields(person);
                    break;
            }

            // Throw error on incomplete broker details
            if (order.BrokerRole != BrokerType.NoBroker && (order.Broker == null || string.IsNullOrWhiteSpace(order.Broker.Name)))
            {
                throw new OpenBookingException(new IncompleteBrokerDetailsError());
            }

            // Reflect back only those broker fields that are supported
            context.Broker = storeBookingEngineSettings.BrokerSupportedFields(order.Broker);

            // Reflect back only those broker fields that are supported
            context.Payment = order.Payment == null ? null : storeBookingEngineSettings.PaymentSupportedFields(order.Payment);

            // Add broker role to context for completeness
            context.BrokerRole = order.BrokerRole;

            // Get static BookingService fields from settings
            context.BookingService = storeBookingEngineSettings.BookingServiceDetails;

            // Create OrderItemContext for each OrderItem
            var orderItemContexts = order.OrderedItem.Select((orderItem, index) => {
                // Error if this group of types is not recognised
                if (!base.IsOpportunityTypeRecognised(orderItem.OrderedItem.Type))
                {
                    // TODO: Update data model to throw actual error for all occurances of OpenBookingError
                    throw new OpenBookingException(new OpenBookingError(), $"The type of opportunity specified is not configured as bookable: '{orderItem.OrderedItem.Type}'.");
                }

                var idComponents = base.ResolveOpportunityID(orderItem.OrderedItem.Type, orderItem.OrderedItem.Id, orderItem.AcceptedOffer.Id);
                if (idComponents == null)
                {
                    // TODO: Update data model to throw actual error for all occurances of OpenBookingError
                    throw new OpenBookingException(new OpenBookingError(), $"Opportunity and Offer ID pair are not in the expected format for a '{orderItem.OrderedItem.Type}': '{orderItem.OrderedItem.Id}' and '{orderItem.AcceptedOffer.Id}'");
                }

                if (idComponents.OpportunityType == null)
                {
                    throw new EngineConfigurationException("OpportunityType must be configured for each IdComponent entry in the settings.");
                }

                // Create the relevant OrderItemContext using the specific type of the IdComponents returned
                Type type = typeof(OrderItemContext<>).MakeGenericType(idComponents.GetType());
                IOrderItemContext orderItemContext = (IOrderItemContext)Activator.CreateInstance(type);
                orderItemContext.Index = index;
                orderItemContext.RequestBookableOpportunityOfferId = idComponents;
                orderItemContext.RequestOrderItem = orderItem;

                return orderItemContext;

            }).ToList();

            // Run a final update outside of the transaction for any records affected that are in RPDE feeds 
            var stateContext = storeBookingEngineSettings.OrderStore.InitialiseFlow(context);

            // Group by OpportunityType for processing
            var orderItemGroups = orderItemContexts.GroupBy(orderItemContext => orderItemContext.RequestBookableOpportunityOfferId.OpportunityType.Value)

            // Get OrderItems first, to check no conflicts exist and that all items are valid
            // Resolve the ID of each OrderItem via a store
            .Select(orderItemContextGroup =>
            {
                var opportunityType = orderItemContextGroup.Key;
                var orderItemContextsWithinGroup = orderItemContextGroup.ToList();
                var store = storeRouting[opportunityType];
                if (store == null)
                {
                    throw new EngineConfigurationException($"Store is not defined for {opportunityType}");
                }

                // QUESTION: Should GetOrderItems occur within the transaction?
                // Currently this is optimised for the transaction to have minimal query coverage (i.e. write-only)

                store.GetOrderItems(orderItemContextsWithinGroup, context);

                if (!orderItemContextsWithinGroup.TrueForAll(x => x.ResponseOrderItem != null))
                {
                    throw new EngineConfigurationException("Not all OrderItemContext have a ResponseOrderItem set. GetOrderItems must always call SetResponseOrderItem for each supplied OrderItemContext.");
                }

                if (!orderItemContextsWithinGroup.TrueForAll(x => x.ResponseOrderItem?.AcceptedOffer?.Price != null && x.ResponseOrderItem?.AcceptedOffer?.PriceCurrency != null))
                {
                    throw new EngineConfigurationException("Not all OrderItemContext have a ResponseOrderItem set with an AcceptedOffer containing both Price and PriceCurrency.");
                }

                // TODO: Implement error logic for all types of item errors based on the results of this

                return new
                {
                    OpportunityType = opportunityType,
                    Store = store,
                    OrderItemContexts = orderItemContextsWithinGroup
                };
            }).ToList();


            // Create a response Order based on the original order of the OrderItems in orderItemContexts
            TOrder responseGenericOrder = new TOrder
            {
                Id = context.OrderIdTemplate.RenderOrderId(context.OrderId),
                BrokerRole = context.BrokerRole,
                Broker = context.Broker,
                Seller = context.Seller,
                Customer = context.Customer,
                BookingService = context.BookingService,
                Payment = context.Payment,
                OrderedItem = orderItemContexts.Select(x => x.ResponseOrderItem).ToList()
            };

            // Add totals to the resulting Order
            OrderCalculations.AugmentOrderWithTotals(responseGenericOrder);


            switch (responseGenericOrder)
            {
                case OrderQuote responseOrderQuote:
                    if (!(context.Stage == FlowStage.C1 || context.Stage == FlowStage.C2))
                        throw new OpenBookingException(new OpenBookingError(), "Unexpected Order type provided");

                    // This library does not yet support approval
                    responseOrderQuote.OrderRequiresApproval = false;

                    // If "payment" has been supplied unnecessarily, simply do not return it
                    if (responseOrderQuote.Payment != null && responseOrderQuote.TotalPaymentDue.Price.Value == 0)
                    {
                        responseOrderQuote.Payment = null;
                    }

                    // Note behaviour here is to lease those items that are available to be leased, and return errors for everything else
                    // Leasing is optimistic, booking is atomic
                    using (IDatabaseTransaction dbTransaction = storeBookingEngineSettings.OrderStore.BeginOrderTransaction(context.Stage))
                    {
                            try
                            {
                                responseOrderQuote.Lease = storeBookingEngineSettings.OrderStore.CreateLease(responseOrderQuote, context, stateContext, dbTransaction);

                                // Lease the OrderItems, if a lease exists
                                if (responseOrderQuote.Lease != null)
                                {
                                    foreach (var g in orderItemGroups)
                                    {
                                        g.Store.LeaseOrderItems(responseOrderQuote.Lease, g.OrderItemContexts, context, dbTransaction);
                                    }
                                }

                                // Update this in case ResponseOrderItem was overwritten in Lease
                                responseOrderQuote.OrderedItem = orderItemContexts.Select(x => x.ResponseOrderItem).ToList();

                                storeBookingEngineSettings.OrderStore.UpdateLease(responseOrderQuote, context, stateContext, dbTransaction);

                                if (dbTransaction != null) dbTransaction.Commit();
                            }
                            catch
                            {
                                if (dbTransaction != null) dbTransaction.Rollback();
                                throw;
                            }
                    }
                    break;

                case Order responseOrder:
                    if (context.Stage != FlowStage.B)
                        throw new OpenBookingException(new OpenBookingError(), "Unexpected Order type provided");

                    // If any capacity errors were returned from GetOrderItems, the booking must fail
                    // https://www.openactive.io/open-booking-api/EditorsDraft/#order-creation-b
                    if (responseOrder.OrderedItem.Any(i => i.Error != null && i.Error.Any(e => e != null && e.GetType() == typeof(OpportunityHasInsufficientCapacityError))))
                    {
                        throw new OpenBookingException(new OpportunityHasInsufficientCapacityError());
                    }

                    // If any lease capacity errors were returned from GetOrderItems, the booking must fail
                    // https://www.openactive.io/open-booking-api/EditorsDraft/#order-creation-b
                    if (responseOrder.OrderedItem.Any(i => i.Error != null && i.Error.Any(e => e != null && e.GetType() == typeof(OpportunityCapacityIsReservedByLeaseError))))
                    {
                        throw new OpenBookingException(new OpportunityCapacityIsReservedByLeaseError());
                    }

                    // If any other errors were returned from GetOrderItems, the booking must fail
                    // https://www.openactive.io/open-booking-api/EditorsDraft/#order-creation-b
                    if (responseOrder.OrderedItem.Any(x => x.Error != null && x.Error.Count > 0))
                    {
                        throw new OpenBookingException(new UnableToProcessOrderItemError());
                    }

                    // If "payment" has been supplied unnecessarily, throw an error
                    if (responseOrder.Payment != null && responseOrder.TotalPaymentDue?.Price == 0)
                    {
                        throw new OpenBookingException(new OpenBookingError(), "UnnecessarilyPaymentDetailsSupplied: Payment details were erroneously supplied for a free Order.");
                    }

                    // Throw error on incomplete broker details
                    if (order.TotalPaymentDue?.Price != responseOrder.TotalPaymentDue?.Price)
                    {
                        throw new OpenBookingException(new TotalPaymentDueMismatchError());
                    }

                    // Booking is atomic
                    using (IDatabaseTransaction dbTransaction = storeBookingEngineSettings.OrderStore.BeginOrderTransaction(context.Stage))
                    {
                        if (dbTransaction == null)
                        {
                            throw new EngineConfigurationException("A transaction is required for booking at B, to ensure the integrity of the booking made.");
                        }

                        try
                        {
                            // Create the parent Order
                            storeBookingEngineSettings.OrderStore.CreateOrder(responseOrder, context, stateContext, dbTransaction);
                            
                            // Book the OrderItems
                            foreach (var g in orderItemGroups)
                            {
                                g.Store.BookOrderItems(g.OrderItemContexts, context, dbTransaction);

                                foreach (var ctx in g.OrderItemContexts)
                                {
                                    // Check that OrderItem Id was added
                                    if (ctx.ResponseOrderItemId == null || ctx.ResponseOrderItem.Id == null )
                                    {
                                        throw new ArgumentException("SetOrderItemId must be called for each OrderItemContext in BookOrderItems");
                                    }

                                    // Set the orderItemStatus to be https://openactive.io/OrderConfirmed (as it must always be so in the response of B)
                                    ctx.ResponseOrderItem.OrderItemStatus = OrderItemStatus.OrderConfirmed;
                                }
                            }

                            // Update this in case ResponseOrderItem was overwritten in Book
                            responseOrder.OrderedItem = orderItemContexts.Select(x => x.ResponseOrderItem).ToList();

                            storeBookingEngineSettings.OrderStore.UpdateOrder(responseOrder, context, stateContext, dbTransaction);

                            dbTransaction.Commit();
                        }
                        catch
                        {
                            dbTransaction.Rollback();
                            throw;
                        }
                    }
                    break;

                default:
                    throw new OpenBookingException(new OpenBookingError(), "Unexpected Order type provided");
            }

            return responseGenericOrder;
        }
    }
}

