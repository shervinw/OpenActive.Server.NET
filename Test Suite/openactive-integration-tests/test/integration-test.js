var assert = require('assert');
var chakram = require('chakram');

process.env["NODE_TLS_REJECT_UNAUTHORIZED"] = 0;

var c1req = require('./c1-req.json');

var expect = chakram.expect;

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