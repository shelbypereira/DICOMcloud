using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using DICOMcloud.Wado;
using DICOMcloud.Wado.Models;


namespace DICOMcloud.Wado.Controllers
{ 

        public class StowRSController : ApiController
    {
        public WebObjectStoreService StorageService { get; set; }

        public StowRSController ( ) : this (null) {}
        public StowRSController ( WebObjectStoreService storageService )
        {
            StorageService = storageService ;
        }
        /// <summary>
        /// Receives the file and saves it, the response returned is defined by the standard:
        /// http://dicom.nema.org/dicom/2013/output/chtml/part18/sect_6.6.html
        /// The RESTful Service shall return an HTTP status line, including a status code and associated textual phrase for the entire set of stored SOP Instances,
        /// followed by an XML message body containing a PS3.19 XML representation of the Store Instances Response Module as defined in Table 6.6.1-2.
        /// </summary>
        /// <param name="studyInstanceUID">The study instance uid.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [HttpPost]
        [Route("stowrs/studies/{studyInstanceUID}")]
        [Route("stowrs")]
        public async Task<HttpResponseMessage> Post(string studyInstanceUID = null)
        {
            WebStoreRequest webStoreRequest = new WebStoreRequest ( Request) ;
            IStudyId studyId = null;


            if ( !string.IsNullOrWhiteSpace (studyInstanceUID))
            { 
                studyId = new ObjectId ( ) {StudyInstanceUID = studyInstanceUID};
            }

            if ( !Request.Content.IsMimeMultipartContent("related") )
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var req=   await Request.Content.ReadAsMultipartAsync ( webStoreRequest ) ;
            
            return await StorageService.Store (req, studyId);
        }
    }
}