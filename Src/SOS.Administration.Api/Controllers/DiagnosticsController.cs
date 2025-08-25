using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Administration.Api.Managers;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Harvest.Helpers.Interfaces;
using SOS.Harvest.Services.Taxon.Interfaces;
using SOS.Lib.Factories;
using SOS.Lib.Helpers;
using System;
using System.Linq;

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
        private readonly DiagnosticsManager _diagnosticsManager;
        private readonly ILogger<DiagnosticsController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="vocabularyHarvester"></param>
        /// <param name="vocabulariesDiffHelper"></param>
        /// <param name="taxonService"></param>
        /// <param name="diagnosticsManager"></param>        
        /// <param name="logger"></param>        
        public DiagnosticsController(
            IVocabularyHarvester vocabularyHarvester,
            IVocabulariesDiffHelper vocabulariesDiffHelper,
            ITaxonService taxonService,
            DiagnosticsManager diagnosticsManager,
            ILogger<DiagnosticsController> logger)
        {
            _vocabularyHarvester =
                vocabularyHarvester ?? throw new ArgumentNullException(nameof(vocabularyHarvester));
            _vocabulariesDiffHelper =
                vocabulariesDiffHelper ?? throw new ArgumentNullException(nameof(vocabulariesDiffHelper));
            _taxonService =
                taxonService ?? throw new ArgumentNullException(nameof(taxonService));
            _diagnosticsManager =
                diagnosticsManager ?? throw new ArgumentNullException(nameof(diagnosticsManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        ///     Get diff between generated, Json files and processed vocabularies.
        /// </summary>
        /// <returns></returns>
        [HttpGet("VocabularyDiffAsZipFile")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetVocabulariesDiffAsZipFile()
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var vocabularyIds = Enum.GetValues(typeof(VocabularyId)).Cast<VocabularyId>();
                var generatedVocabularies =
                    await _vocabularyHarvester.CreateAllVocabulariesAsync(vocabularyIds);
                var zipBytes = await _vocabulariesDiffHelper.CreateDiffZipFile(generatedVocabularies);
                return File(zipBytes, "application/zip", "VocabularyDiffBetweenVerbatimAndProcessed.zip");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{@methodName}() failed", MethodBase.GetCurrentMethod()?.Name);                
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
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
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
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
                _logger.LogError(e, "{@methodName}() failed", MethodBase.GetCurrentMethod()?.Name);                
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
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var dwcTaxa = await _taxonService.GetTaxaAsync();
                var taxa = dwcTaxa.ToProcessedTaxa();
                Lib.Models.TaxonTree.TaxonTree<Lib.Models.Interfaces.IBasicTaxon> taxonTree = TaxonTreeFactory.CreateTaxonTree(taxa);

                Result<string> strGraphviz = null;
                if (diagramFormat == DiagramFormat.GraphViz)
                {
                    strGraphviz = TaxonRelationDiagramHelper.CreateGraphvizFormatRepresentation(
                        taxonTree,
                        taxonIds,
                        null,
                        treeIterationMode,
                        includeSecondaryRelations);
                }
                else if (diagramFormat == DiagramFormat.Mermaid)
                {
                    strGraphviz = TaxonRelationDiagramHelper.CreateMermaidFormatRepresentation(
                        taxonTree,
                        taxonIds,
                        null,
                        treeIterationMode,
                        includeSecondaryRelations);
                }

                if (strGraphviz.IsFailure) return BadRequest(strGraphviz.Error);

                return Ok(strGraphviz);
                //return File(System.Text.Encoding.UTF8.GetBytes(strGraphviz),"text/plain", "TaxonCategory Diagram.gv");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{@methodName}() failed", MethodBase.GetCurrentMethod()?.Name);                
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        ///     Get taxa categories summary.
        /// </summary>
        /// <returns></returns>
        [HttpGet("TaxaSummary")]
        //[ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTaxaSummary(string collectionName = "Taxon")
        {
            try
            {
                var json = await _diagnosticsManager.CreateBasicTaxaSummaryJsonAsync(collectionName);
                return File(System.Text.Encoding.UTF8.GetBytes(json),"text/plain", "TaxaSummary.json");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{@methodName}() failed", MethodBase.GetCurrentMethod()?.Name);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
        
    }
}