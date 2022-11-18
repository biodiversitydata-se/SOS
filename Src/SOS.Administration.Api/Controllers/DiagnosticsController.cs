using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Harvest.Helpers.Interfaces;
using SOS.Harvest.Services.Taxon.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Factories;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    ///     Diagnostics controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DiagnosticsController : ControllerBase, IDiagnosticsController
    {
        private readonly IVocabulariesDiffHelper _vocabulariesDiffHelper;
        private readonly IVocabularyHarvester _vocabularyHarvester;
        private readonly ITaxonService _taxonService;
        private readonly ILogger<DiagnosticsController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="vocabularyHarvester"></param>
        /// <param name="vocabulariesDiffHelper"></param>
        /// <param name="taxonService"></param>
        /// <param name="logger"></param>        
        public DiagnosticsController(
            IVocabularyHarvester vocabularyHarvester,
            IVocabulariesDiffHelper vocabulariesDiffHelper,
            ITaxonService taxonService,
            ILogger<DiagnosticsController> logger)
        {
            _vocabularyHarvester =
                vocabularyHarvester ?? throw new ArgumentNullException(nameof(vocabularyHarvester));
            _vocabulariesDiffHelper =
                vocabulariesDiffHelper ?? throw new ArgumentNullException(nameof(vocabulariesDiffHelper));
            _taxonService =
                taxonService ?? throw new ArgumentNullException(nameof(taxonService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        ///     Get diff between generated, Json files and processed vocabularies.
        /// </summary>
        /// <returns></returns>
        [HttpGet("VocabularyDiffAsZipFile")]
        [ProducesResponseType(typeof(byte[]), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetVocabulariesDiffAsZipFile()
        {
            try
            {
                var vocabularyIds = Enum.GetValues(typeof(VocabularyId)).Cast<VocabularyId>();
                var generatedVocabularies =
                    await _vocabularyHarvester.CreateAllVocabulariesAsync(vocabularyIds);
                var zipBytes = await _vocabulariesDiffHelper.CreateDiffZipFile(generatedVocabularies);
                return File(zipBytes, "application/zip", "VocabularyDiffBetweenVerbatimAndProcessed.zip");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod()?.Name}() failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        ///     Get Taxon category relations as diagram.
        /// </summary>
        /// <returns></returns>
        [HttpGet("TaxonCategoryDiagram")]
        //[ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTaxonCategoryDiagram(
            [FromQuery] DiagramFormat diagramFormat = DiagramFormat.GraphViz,
            [FromQuery] bool includeSecondaryRelations = false)
        {
            try
            {
                var dwcTaxa = await _taxonService.GetTaxaAsync();
                var dwcTaxonById = dwcTaxa.ToDictionary(m => m.Id, m => m);
                var taxonCategories = TaxonCategoryHelper.GetTaxonCategories(dwcTaxonById);
                var edges = TaxonCategoryHelper.GetTaxonCategoryEdges(taxonCategories);
                string strGraphviz = null; 
                if (diagramFormat == DiagramFormat.GraphViz)
                {
                    strGraphviz = TaxonCategoryHelper.CreateGraphVizDiagram(edges, includeSecondaryRelations);
                }
                else if (diagramFormat == DiagramFormat.Mermaid)
                {
                    strGraphviz = TaxonCategoryHelper.CreateMermaidDiagram(edges, includeSecondaryRelations);
                }
                
                return Ok(strGraphviz);
                //return File(System.Text.Encoding.UTF8.GetBytes(strGraphviz),"text/plain", "TaxonCategory Diagram.gv");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod()?.Name}() failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        ///     Get Taxon relations as diagram.
        /// </summary>
        /// <returns></returns>
        [HttpGet("TaxonRelationsDiagram")]
        //[ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTaxonRelationsDiagram(
            [FromQuery] int[] taxonIds,
            [FromQuery] TaxonRelationDiagramHelper.TaxonRelationsTreeIterationMode treeIterationMode = TaxonRelationDiagramHelper.TaxonRelationsTreeIterationMode.BothParentsAndChildren,
            [FromQuery] bool includeSecondaryRelations = false,            
            [FromQuery] DiagramFormat diagramFormat = DiagramFormat.Mermaid)
        {
            try
            {
                var dwcTaxa = await _taxonService.GetTaxaAsync();
                var taxa = dwcTaxa.ToProcessedTaxa();
                var taxonTree = TaxonTreeFactory.CreateTaxonTree((IDictionary<int, IBasicTaxon>)taxa);

                string strGraphviz = null;
                if (diagramFormat == DiagramFormat.GraphViz)
                {
                    strGraphviz = TaxonRelationDiagramHelper.CreateGraphvizFormatRepresentation(
                        taxonTree,
                        taxonIds,
                        treeIterationMode,
                        includeSecondaryRelations);
                }
                else if (diagramFormat == DiagramFormat.Mermaid)
                {
                    strGraphviz = TaxonRelationDiagramHelper.CreateMermaidFormatRepresentation(
                        taxonTree,
                        taxonIds,
                        treeIterationMode,
                        includeSecondaryRelations);
                }

                return Ok(strGraphviz);
                //return File(System.Text.Encoding.UTF8.GetBytes(strGraphviz),"text/plain", "TaxonCategory Diagram.gv");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod()?.Name}() failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}