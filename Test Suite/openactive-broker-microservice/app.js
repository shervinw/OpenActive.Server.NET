const DATA_MODEL_OUTPUT_DIR = "../OpenActive.DatasetSite.NET/metadata/";
const DATASET_SITE_TEMPLATE_URL = "https://www.openactive.io/dataset-site-template/datasetsite.2.mustache";

const { getModels, getEnums, getMetaData } = require('@openactive/data-models');
var fs = require('fs');
var fsExtra = require('fs-extra');
var request = require('sync-request');
var path = require('path');

removeFiles()
generateDatasetSiteMustacheTemplate(DATASET_SITE_TEMPLATE_URL);
generateFeedConfigurations();

function removeFiles() {
    // Empty output directories
    fsExtra.emptyDirSync(DATA_MODEL_OUTPUT_DIR);
}

function generateDatasetSiteMustacheTemplate (datasetSiteTemplateUrl) {
    var content = getContentFromUrl(datasetSiteTemplateUrl);
    if (content) {
        writeFile('DatasetSiteMustacheTemplate', renderMustacheTemplateFile(content));
    }
}

function renderMustacheTemplateFile(content) {
    return `
namespace OpenActive.DatasetSite.NET
{
    public static class DatasetSiteMustacheTemplate
    {
        public const string Content = @"
${content.replace(/\"/g, '""')}
";
    }
}
`;
}

function generateFeedConfigurations() {
    // Returns the latest version of the models map
    const feedConfigurations = getMetaData().feedConfigurations;

    if (feedConfigurations) {
        writeFile('FeedConfigurations', renderFeedConfigurations(feedConfigurations));
    }
}

function renderFeedConfigurations(feedConfigurations) {
    return `
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenActive.DatasetSite.NET
{
    public enum FeedType
    {
        ${feedConfigurations.map(c => c.name).join(`,
        `)}
    }

    public static class FeedConfigurations
    {
        public readonly static Dictionary<FeedType, FeedConfiguration> Configurations = new Dictionary<FeedType, FeedConfiguration>
        {${feedConfigurations.map(c => `
            {
                FeedType.${c.name},
                new FeedConfiguration {
                    Name = "${c.name}",
                    SameAs = new Uri("${c.sameAs}"),
                    DefaultFeedPath = "${c.defaultFeedPath}",
                    PossibleKinds = new List<string> { ${c.possibleKinds.map(k => `"${k}"`).join(', ')} }${c.displayName ? `,
                    DisplayName = "${c.displayName}"` : ''}
                }
            }`).join(',')}
        };
    }
}
`
}

function getContentFromUrl(url) {
    var response = request('GET', url, { accept: 'text/html' });
    if (response && response.statusCode == 200) {
        return response.getBody('utf8');
    } else {
        return undefined;
    }
}

function writeFile(name, content) {
    var filename = name + ".cs";
    
    console.log("NAME: " + filename);
    console.log(content);

    fs.writeFile(DATA_MODEL_OUTPUT_DIR + filename, content, function (err) {
        if (err) {
            return console.log(err);
        }

        console.log("FILE SAVED: " + filename);
    });
}
