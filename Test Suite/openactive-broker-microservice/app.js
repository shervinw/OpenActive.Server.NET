var express = require('express');
var logger = require('morgan');
var request = require('request');
 
process.env["NODE_TLS_REJECT_UNAUTHORIZED"] = 0;
process.env["PORT"] = 3000;

var app = express();

app.use(logger('dev'));
app.use(express.json());

function getRPDE(url, cb) {
  var headers = {
    'Accept': 'application/json',
    'Content-Type': 'application/json',
    'Cache-Control' : 'max-age=0'
  };
  var options = {
    'method': 'get',
    'headers': headers
  };
  request.get({ url }, function(error, response, body) { 
    if (!error && response.statusCode == 200) { 
      cb(JSON.parse(body));
    } else {
      console.log("Error for RPDE page: " + error);
      // Fake next page to force retry, after a delay
      setTimeout(cb({ next: url, items: [] }), 5000);
    }
  });
}

function getBaseUrl(url) {
  if (url.indexOf("//") > -1) {
    return url.substring(0, url.indexOf("/", url.indexOf("//") + 2));
  } else {
    return ""
  }
}

var responses = {
  /* Keyed by expression =*/
};

var requestCounter = 0;

app.get('/get-match/:expression', function (req, res) {
  // respond with json
  if (req.params.expression) {
    requestCounter += 1;
    var expression = req.params.expression;

    // Stash the response and reply later when an event comes through (kill any existing expression still waiting)
    if (responses[expression] && responses[expression] !== null) responses[expression].end();
    responses[expression] = {
      send: function(json) {
        responses[expression] = null;
        res.json(json);
        res.end();
      },
      end: function() {
        res.end();
      },
      res
    };
  } else {
    res.send("Expression not valid");
  }
});

var nextUrl = 'https://localhost:44307/feeds/session-series';
var pageNumber = 0;

// Start processing first page
getRPDE(nextUrl, processPage);

function processPage(rpde) {
  pageNumber++;

  console.log(`RPDE page: ${pageNumber}, length: ${rpde.items.length}, next: '${rpde.next}'`);

  rpde.items.forEach((item) => {
    // TODO: make this regex loop (note ignore deleted items)
    if (item.data && responses[item.data.name]) {
      responses[item.data.name].send(item);
    }
  });

  setTimeout(x => getRPDE(rpde.next, processPage), 200);
}

app.listen(3000, '127.0.0.1');
console.log('Node server running on port 3000');