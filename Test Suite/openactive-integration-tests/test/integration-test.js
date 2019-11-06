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

    before(function () {
        apiResponse = chakram.get("http://localhost:3000/get-match/Testevent2");

        testEvent = {
            type: "Event",
            name: "Testevent2"
        };

        delay(500).then(x => chakram.post("https://localhost:44307/api/openbooking/test-interface/create", testEvent));

        return apiResponse;
    });

    after(function () {
        id = apiResponse.body['@id'];

        return chakram.delete("http://localhost:3000/delete/" + encodeURIComponent(id));
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
        return expect(apiResponse).to.have.json('data.offers[0].price', '2');
    });

});