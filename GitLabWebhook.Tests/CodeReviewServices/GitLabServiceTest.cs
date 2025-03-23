﻿using Castle.Core.Configuration;
using CodeReviewServices;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitLabWebhook.Tests.CodeReviewServices
{
   

    public class GitLabServiceTest
    {

        private readonly GitLabService _gitLabService;

        public GitLabServiceTest()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Set the base path to the current directory (tests folder)
               .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "config.json"), optional: false, reloadOnChange: true);

            var configuration = configurationBuilder.Build();
            _gitLabService = new GitLabService(configuration);
        }

        [Theory]
        [InlineData("https://gitlab.dell.com/seller/dsa/production/DSAPlatform/qto-quote-create/draft-quote/DSA-CartService/-/merge_requests/1375",
            "seller/dsa/production/DSAPlatform/qto-quote-create/draft-quote/DSA-CartService")]
        [InlineData("https://gitlab.dell.com/seller/dsa/production/DSAPlatform/qto-quote-create/draft-quote/DSA-CartService/merge_requests/1375",
            "seller/dsa/production/DSAPlatform/qto-quote-create/draft-quote/DSA-CartService")]
        public void ShouldParseProjectName(String inputURL, String expectedResult)
        {

            //Arrange
            // All of the arranging was done in the Theory

            //Act
            var projectPath = GitLabService.GetProjectPathFromUrl(inputURL);

            //Assert
            Assert.Equal(expectedResult, projectPath);
        }


    }

   
}



