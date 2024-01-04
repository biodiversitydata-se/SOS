﻿using Microsoft.AspNetCore.Mvc;
using SOS.Administration.Gui.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Administration.Gui.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        private readonly ITestService _service;

        public TestsController(ITestService testService)
        {
            _service = testService;
        }
        [HttpGet]
        public IEnumerable<Test> GetTests()
        {
            return _service.GetTests();
        }
        [HttpGet]
        [Route("Test_SearchOtter")]
        public async Task<TestResults> Test_SearchOtter()
        {
            return await _service.Test_SearchOtter();
        }

        [HttpGet]
        [Route("Test_SearchOtterAtLocation")]
        public async Task<TestResults> Test_SearchOtterAtLocation()
        {
            return await _service.Test_SearchOtterAtLocation();
        }
        [HttpGet]
        [Route("Test_SearchWolf")]
        public async Task<TestResults> Test_SearchWolf()
        {
            return await _service.Test_SearchWolf();
        }

        [HttpGet]
        [Route("Test_GeoGridAggregation")]
        public async Task<TestResults> Test_GeoGridAggregation()
        {
            return await _service.Test_GeoGridAggregation();
        }
        [HttpGet]
        [Route("Test_TaxonAggregation")]
        public async Task<TestResults> Test_TaxonAggregation()
        {
            return await _service.Test_TaxonAggregation();
        }
        [HttpGet]
        [Route("Test_TaxonAggregationBBox")]
        public async Task<TestResults> Test_TaxonAggregationBBox()
        {
            return await _service.Test_TaxonAggregationBBox();
        }
        [HttpGet]
        [Route("Test_DataProviders")]
        public async Task<TestResults> Test_DataProviders()
        {
            return await _service.Test_DataProviders();
        }

        [HttpGet]
        [Route("Test_Vocabularies")]
        public async Task<TestResults> Test_Vocabulary()
        {
            return await _service.Test_Vocabulary();
        }
    }
}
