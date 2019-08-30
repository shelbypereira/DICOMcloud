﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using fo = Dicom;
using Dicom.Imaging.Codec;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web;
using System.Web.SessionState;
using DICOMcloud.DataAccess.UnitTest;
using Dicom;
using DICOMcloud.Pacs;
using DICOMcloud.Media;
using DICOMcloud.IO;
using DICOMcloud.DataAccess;
using DICOMcloud.DataAccess.Database;
using DICOMcloud.DataAccess.Database.Schema;
using DICOMcloud.Wado;
using DICOMcloud.Wado.Models;

namespace DICOMcloud.UnitTest
{
    [TestClass]
    public class PaginationSearchTest
    {
        /// <summary>
        /// force empty the database before running any tests this avoids issues when we debug tests and exit
        /// before the test finishes
        /// </summary>
        /// <param name="context">The context.</param>
        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            try
            {
             new DataAccessHelpers().EmptyDatabase();
            }
            catch
            {

            }
        }

        readonly string storagePath = DicomHelpers.GetTestDataFolder("storage", true);
        [TestInitialize]
        public void Initialize ( ) 
        {
            try
            { 
                DicomHelper      = new DicomHelpers ( ) ;
                DataAccessHelper = new DataAccessHelpers ( ) ;

               
                var mediaIdFactory = new DicomMediaIdFactory();


                MediaStorageService storageService = new FileStorageService(storagePath);

                var factory = new Pacs.Commands.DCloudCommandFactory(storageService,
                                                                     DataAccessHelper.DataAccess,
                                                                     new DicomMediaWriterFactory(storageService,
                                                                                                   mediaIdFactory),
                                                                     mediaIdFactory);

                StoreService = new ObjectStoreService(factory);
                QueryService = new ObjectArchieveQueryService (DataAccessHelper.DataAccess);

                PopulateData ( ) ;
            }
            catch ( Exception )
            {
                Cleanup ( ) ;

                throw ;
            }
        }

        [TestCleanup]
        public void Cleanup ( )
        {
            DataAccessHelper.EmptyDatabase ( ) ;
        }
         static HttpContext FakeHttpContext()
        {
            var httpRequest = new HttpRequest("", "http://localhost:44301/", "");
            var stringWriter = new StringWriter();
            var httpResponse = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponse);
            /*
            var sessionContainer = new HttpSessionStateContainer("id", new SessionStateItemCollection(),
                new HttpStaticObjectsCollection(), 10, true,
                HttpCookieMode.AutoDetect,
                SessionStateMode.InProc, false);

            httpContext.Items["AspSession"] = typeof(HttpSessionState).GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null, CallingConventions.Standard,
                    new[] { typeof(HttpSessionStateContainer) },
                    null)
                .Invoke(new object[] { sessionContainer });
            */
            return httpContext;
        }
        /// <summary>
        /// simulates the Dicomweb-JS button push "Search on the Query & retrieve page when all the search fields are blank
        /// </summary>
        [TestMethod]
        public void QidoControllerSearchForStudiesEmptyParams()
        {
            QidoRsService service = new QidoRsService(QueryService, new DicomMediaIdFactory(),
                new FileStorageService(storagePath));
            QidoRequestModel request = new QidoRequestModel();
            // setup the parameters so that they are the same as the default request sent with empty params from the search page (Query and Retrieve)
            request.Limit = 12;
            request.Offset = 0;
            request.Query = new QidoQuery();
            request.Query.IncludeElements.Add("00200010");
            request.Query.IncludeElements.Add("00080020");
            request.Query.IncludeElements.Add("00100010");
            request.Query.IncludeElements.Add("00100020");
            var client = new HttpClient();
            var headers = client.DefaultRequestHeaders;
            request.AcceptCharsetHeader =client.DefaultRequestHeaders.AcceptCharset;
            request.AcceptHeader = client.DefaultRequestHeaders.Accept;
            request.AcceptHeader.Add(new MediaTypeWithQualityHeaderValue( "application/dicom+json"));
            //the following is needed since the SearchForSeries will use HttpContext.Current to get the uri
            //and implement pagination
            HttpContext.Current = FakeHttpContext();
            service.SearchForSeries(request);
        }


        /// <summary>
        /// force instantiation of the DefaultDicomQueryElements class,  the static constructor
        /// had some issues with the types in the underlying dataset, this test method throws an exception if that setup fails
        /// </summary>
        [TestMethod]
        public void TestDicomQueryElements()
        {
            DefaultDicomQueryElements.GetDefaultStudyQuery();
            DefaultDicomQueryElements.GetDefaultInstanceQuery();
            DefaultDicomQueryElements.GetDefaultSeriesQuery();
        }

        [TestMethod]
        public void QueryStudiesPagedNormalCase ( )
        {
            int expectedNumberOfPages = 3;
            int expectedPageNumber = 1;
            int expectedNumberOfResults = TotalNumberOfStudies;
            var limit = 3;
            var requestDS = new DicomDataset();
            var options = new QueryOptions() { Limit = limit, Offset = 0 };

            var pagedResult = QueryService.FindStudiesPaged(requestDS, options);

            ValidatePagedResult (expectedNumberOfPages, expectedPageNumber, limit, expectedNumberOfResults, pagedResult);
        }

        [TestMethod]
        public void QueryStudiesPagedNormalIncludeSeriesLevelModality()
        {
            int expectedNumberOfPages = 3;
            int expectedPageNumber = 1;
            int expectedNumberOfResults = TotalNumberOfStudies;
            var limit = 3;
            var requestDS = new DicomDataset();
            var options = new QueryOptions() { Limit = limit, Offset = 0 };


            requestDS.AddOrUpdate(DicomTag.Modality, "");
            var pagedResult = QueryService.FindStudiesPaged(requestDS, options);

            ValidatePagedResult(expectedNumberOfPages, expectedPageNumber, limit, expectedNumberOfResults, pagedResult);

            // We have only 3 studies out of 9 with CT
            expectedNumberOfResults = 3;
            expectedNumberOfPages = 1;
            requestDS.AddOrUpdate(DicomTag.Modality, "CT");
            pagedResult = QueryService.FindStudiesPaged(requestDS, options);

            ValidatePagedResult(expectedNumberOfPages, expectedPageNumber, limit, expectedNumberOfResults, pagedResult);
        }

        [TestMethod]
        public void QueryStudiesPagedNormalIncludeSeriesLevelModalityFailed()
        {
            int expectedNumberOfPages = 3;
            int expectedPageNumber = 1;
            // This will return 3 studies x 3 Series = 9 which is wrong but expected for this test
            int expectedNumberOfResults = 9;
            var limit = 3;
            var requestDS = new DicomDataset();
            var options = new QueryOptions() { Limit = limit, Offset = 0 };


            // This exposes an implementation problem for applying pagination on the database
            // when lower level tables are joined/requested in the query. Issue #25
            // This is now fixed by applying the pagination in code for this case.
            var sortingFactory = new CustomSortingStrategyFactory (((ObjectArchieveDataAccess)QueryService.QueryDataAccess).SchemaProvider);
            ((ObjectArchieveDataAccess)QueryService.QueryDataAccess).DataAdapter.SortingStrategyFactory = sortingFactory;

            var pagedResult = QueryService.FindStudiesPaged(requestDS, options);

            ValidatePagedResult(expectedNumberOfPages, expectedPageNumber, limit, expectedNumberOfResults, pagedResult);
        }

        private void ValidatePagedResult
        (   
            int numberOfPages, 
            int pageNumber, 
            int limit, 
            int numberOfResults,
            PagedResult<DicomDataset> pagedResult
        )
        {
            Assert.AreEqual(numberOfPages, pagedResult.NumberOfPages);
            Assert.AreEqual(pageNumber, pagedResult.PageNumber);
            Assert.AreEqual(limit, pagedResult.PageSize);
            Assert.AreEqual(numberOfResults, pagedResult.TotalCount);
        }

        private void PopulateData()
        {
            var templateDS = DicomHelper.GetDicomDataset(0) ;
            var modalities = new string[] { "CT", "MR", "XA"};

            for (int studyIndex = 0; studyIndex < TotalNumberOfStudies; studyIndex++)
            {
                
                var studyDs = new DicomDataset ( ); 
                
                
                templateDS.CopyTo ( studyDs );

                studyDs.AddOrUpdate (DicomTag.StudyInstanceUID, string.Format ("999.{0}", studyIndex));

                for (int seriesIndex = 0; seriesIndex  < NumberOfSeriesInStudy; seriesIndex++)
                {
                    var seriesDs = new DicomDataset ( );


                    studyDs.CopyTo (seriesDs);

                    seriesDs.AddOrUpdate (DicomTag.SeriesInstanceUID, string.Format ("333.{0}.444.{1}", studyIndex, seriesIndex));
                    seriesDs.AddOrUpdate(DicomTag.Modality, modalities[studyIndex% modalities.Length]);
                    
                    for (int instanceIndex = 0; instanceIndex < NumberOfInstancesInSeries; instanceIndex++ )
                    { 
                        var instanceDs = new DicomDataset ( ) ;


                        seriesDs.CopyTo (instanceDs);

                        instanceDs.AddOrUpdate (DicomTag.SOPInstanceUID, string.Format ("333.{0}.444.{1}.555.{2}", studyIndex, seriesIndex, instanceIndex));

                        StoreService.StoreDicom(instanceDs, new DataAccess.InstanceMetadata());
                    }
                }
            }
        }

        int TotalNumberOfStudies = 9;
        int NumberOfSeriesInStudy = 3;
        int NumberOfInstancesInSeries = 1;

        private DicomHelpers DicomHelper { get; set; }
        private DataAccessHelpers DataAccessHelper { get; set; }
        private IObjectStoreService StoreService { get; set; }
        private ObjectArchieveQueryService QueryService { get; set; }


        // This class exposes an implementation problem for applying pagination on the database
        // when lower level tables are joined/requested in the query. Issue #25
        class CustomObjectArchieveSortingStrategy : ObjectArchieveSortingStrategy
        {
            public CustomObjectArchieveSortingStrategy ( DbSchemaProvider schemaProvider ) 
            : base (schemaProvider)
            { }

            public override bool CanPaginate
            (
                QueryBuilder queryBuilder, 
                IQueryOptions options, 
                TableKey queryLeveTable
            )
            {
                return (null != options && options.Limit > 0) ;
            }
        }

        class CustomSortingStrategyFactory : SortingStrategyFactory
        {
            public CustomSortingStrategyFactory (DbSchemaProvider schemaProvier)
            : base (schemaProvier)
            { 
            }

            public override ISortingStrategy Create()
            {
                return new CustomObjectArchieveSortingStrategy (this.SchemaProvider);
            }
        }
    }
}
