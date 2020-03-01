// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using OpenActive.FakeDatabase.NET;

namespace src
{
    /// <summary>
    /// This sample controller allows a user to revoke grants given to clients
    /// </summary>
    [SecurityHeaders]
    [Authorize]
    public class BookingPartnersController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clients;
        private readonly IResourceStore _resources;
        private readonly IEventService _events;

        public BookingPartnersController(IIdentityServerInteractionService interaction,
            IClientStore clients,
            IResourceStore resources,
            IEventService events)
        {
            _interaction = interaction;
            _clients = clients;
            _resources = resources;
            _events = events;
        }

        /// <summary>
        /// Show list of grants
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View("Index", await BuildViewModelAsync());
        }

        /// <summary>
        /// Handle postback to revoke a client
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Revoke(string clientId)
        {
            await _interaction.RevokeUserConsentAsync(clientId);
            await _events.RaiseAsync(new GrantsRevokedEvent(User.GetSubjectId(), clientId));

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Handle postback to revoke a client
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(string clientId)
        {
            var client = await _clients.FindClientByIdAsync(clientId);
            client.AllowedScopes.Remove("openactive-openbooking");
            await _events.RaiseAsync(new GrantsRevokedEvent(User.GetSubjectId(), clientId));

            var bookingPartner = FakeBookingSystem.Database.BookingPartners.FirstOrDefault(t => t.ClientId == clientId);
            bookingPartner.ClientJson.Scope = "openid profile openactive-ordersfeed oauth-dymamic-client-update openactive-identity";
            bookingPartner.BookingsSuspended = true;

            return RedirectToAction("Index");
        }

        private async Task<BookingPartnerViewModel> BuildViewModelAsync()
        {
            var grants = await _interaction.GetAllUserConsentsAsync();

            var list = new List<BookingPartnerModel>();
            foreach(var grant in grants)
            {
                var client = await _clients.FindClientByIdAsync(grant.ClientId);
                if (client != null)
                {
                    var resources = await _resources.FindResourcesByScopeAsync(grant.Scopes);
                    var bookingPartner =  FakeBookingSystem.Database.BookingPartners.FirstOrDefault(t => t.ClientId == client.ClientId);

                    var item = new BookingPartnerModel()
                    {
                        ClientId = client.ClientId,
                        ClientName = client.ClientName ?? client.ClientId,
                        ClientLogoUrl = bookingPartner.ClientJson.LogoUri,
                        ClientUrl = bookingPartner.ClientJson.ClientUri,
                        Created = grant.CreationTime,
                        Expires = grant.Expiration,
                        IdentityGrantNames = resources.IdentityResources.Select(x => x.DisplayName ?? x.Name).ToArray(),
                        ApiGrantNames = resources.ApiResources.Select(x => x.DisplayName ?? x.Name).ToArray(),
                        BookingPartner = bookingPartner
                    };

                    list.Add(item);
                }
            }

            return new BookingPartnerViewModel
            {
                BookingPartners = list
            };
        }
    }
}