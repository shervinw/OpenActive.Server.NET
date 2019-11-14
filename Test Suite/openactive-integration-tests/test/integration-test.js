const assert = require('assert');
const chakram = require('chakram');
const mustache = require('mustache');
const uuidv5 = require('uuid/v5');

process.env["NODE_TLS_REJECT_UNAUTHORIZED"] = 0;

const c1req = require('./c1-req.json');
const c2req = require('./c2-req.json');
const breq = require('./b-req.json');

var BOOKING_API_BASE = 'https://localhost:44307/api/openbooking/';

var expect = chakram.expect;

function bookingRequest(templateJson, replacementMap) {
    var template = JSON.stringify(templateJson, null, 2);

    var req = mustache.render(template, replacementMap);

    console.log("\n\n** REQUEST **: \n\n" + req);

    return JSON.parse(req);
}


function delay(t, v) {
    return new Promise(function(resolve) { 
        setTimeout(resolve.bind(null, v), t)
    });
 }

describe("Create test event", function() {
    this.timeout(10000);

    var apiResponse;

    var testEvent = {
        "@type": "Event",
        name: "Testevent2",
        "offers": [
            {
                "@type": "Offer",
                "price": 2,
            }
        ]
    };

    before(function () {
        apiResponse = chakram.get("http://localhost:3000/get-match/Testevent2");

        delay(500).then(x => chakram.post("https://localhost:44307/api/openbooking/test-interface/scheduledsession", testEvent, {
            headers: {
                'Content-Type': 'application/vnd.openactive.booking+json; version=1'
             }
        }));

        return apiResponse;
    });

    after(function () {
        var name = testEvent.name;
        return chakram.delete("https://localhost:44307/api/openbooking/test-interface/scheduledsession/" + encodeURIComponent(name), {
            headers: {
                'Content-Type': 'application/vnd.openactive.booking+json; version=1'
             }
        });
    });

    it("should return 200 on success", function () {
        return expect(apiResponse).to.have.status(200);
    });

    it("should return newly created event", function () {
        expect(apiResponse).to.have.json('data.@type', 'ScheduledSession');
        expect(apiResponse).to.have.json('data.name', 'Testevent2');
        return chakram.wait();
    });

    it("should have one offer", function () {
        return expect(apiResponse).to.have.schema('data.offers', {minItems: 1, maxItems: 1});
    });
    
    it("offer should have price of 2", function () {
        return expect(apiResponse).to.have.json('data.offers[0].price', 2);
    });

});


describe("Basic end-to-end booking", function() {
    this.timeout(10000);

    var testEvent = {
        "@type": "Event",
        name: "Testevent2",
        "offers": [
            {
                "@type": "Offer",
                "price": 2,
            }
        ]
    };

    var apiResponse;
    var opportunityId;
    var offerId;
    var sellerId;
    var uuid;

    var c1Response;
    var c2Response;
    var bResponse;

    before(function () {
        apiResponse = chakram.get("http://localhost:3000/get-match/Testevent2").then(function(respObj) {
            var rpdeItem = respObj.body;
            console.log("\n\n** RPDE excerpt **: \n\n" + JSON.stringify(rpdeItem, null, 2));

            opportunityId = rpdeItem.data['@id']; // TODO : Support duel feeds: .subEvent[0]
            offerId = rpdeItem.data.offers[0]['@id'];
            sellerId = rpdeItem.data.organizer['@id'];
            uuid = uuidv5(sellerId, uuidv5.URL); //uuid v5 based on Seller ID

            console.log(`opportunityId: ${opportunityId}; offerId: ${offerId}`)
        }).then(x => chakram.put(BOOKING_API_BASE + 'order-quote-templates/' + uuid, bookingRequest(c1req, {
            opportunityId,
            offerId,
            sellerId,
            uuid
        }), {
            headers: {
                'Content-Type': 'application/vnd.openactive.booking+json; version=1'
             }
        })).then(function(respObj) {
            c1Response = respObj;
            console.log("\n\n** C1 response: ** \n\n" + JSON.stringify(c1Response.body, null, 2));
            //var total = c1Response.totalPaymentDue.price;
        }).then(x => chakram.put(BOOKING_API_BASE + 'order-quotes/' + uuid, bookingRequest(c2req, {
            opportunityId,
            offerId,
            sellerId,
            uuid
        }), {
            headers: {
                'Content-Type': 'application/vnd.openactive.booking+json; version=1'
             }
        })).then(function(respObj) {
            c2Response = respObj;
            console.log("\n\n** C2 response: ** \n\n" + JSON.stringify(c2Response.body, null, 2));
            //var total = c2Response.totalPaymentDue.price;
        }).then(x => chakram.put(BOOKING_API_BASE + 'orders/' + uuid, bookingRequest(breq, {
            opportunityId,
            offerId,
            sellerId,
            uuid
        }), {
            headers: {
                'Content-Type': 'application/vnd.openactive.booking+json; version=1'
             }
        })).then(function(respObj) {
            bResponse = respObj;
            console.log("\n\n** B response: **\n\n" + JSON.stringify(bResponse.body, null, 2));
            //var total = bResponse.totalPaymentDue.price;
        });


        delay(500).then(x => chakram.post("https://localhost:44307/api/openbooking/test-interface/scheduledsession", testEvent, {
            headers: {
                'Content-Type': 'application/vnd.openactive.booking+json; version=1'
             }
        }));

        return apiResponse;
    });

    after(function () {
        var name = testEvent.name;
        return chakram.delete("https://localhost:44307/api/openbooking/test-interface/scheduledsession/" + encodeURIComponent(name), {
            headers: {
                'Content-Type': 'application/vnd.openactive.booking+json; version=1'
             }
        }).then(x => chakram.delete(BOOKING_API_BASE + 'orders/' + uuid, {
            headers: {
                'Content-Type': 'application/vnd.openactive.booking+json; version=1'
             }
        }));
    });

    it("should return 200 on success", function () {
        expect(c1Response).to.have.status(200);
        expect(c2Response).to.have.status(200);
        expect(bResponse).to.have.status(200);
        return chakram.wait();
    });

    it("should return newly created event", function () {
        expect(apiResponse).to.have.json('orderedItem[0].orderedItem.@type', 'ScheduledSession');
        expect(apiResponse).to.have.json('orderedItem[0].orderedItem.superEvent.name', 'Testevent2');
        return chakram.wait();
    });

    it("should have one offer", function () {
        expect(c1Response).to.have.schema('data.offers', {minItems: 1, maxItems: 1});
        expect(c2Response).to.have.schema('data.offers', {minItems: 1, maxItems: 1});
        expect(bResponse).to.have.schema('data.offers', {minItems: 1, maxItems: 1});
        return chakram.wait();
    });
    
    it("offer should have price of 2", function () {
        expect(c1Response).to.have.json('orderedItem[0].acceptedOffer.price', 2);
        expect(c2Response).to.have.json('orderedItem[0].acceptedOffer.price', 2);
        expect(bResponse).to.have.json('orderedItem[0].acceptedOffer.price', 2);
        return chakram.wait();
    });

});