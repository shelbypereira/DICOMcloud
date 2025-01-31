﻿using Dicom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fo = Dicom;


namespace DICOMcloud.UnitTest
{
    public class DicomHelpers
    {
        public DicomHelpers ( ) 
        {
            // the new fo-dicom 4.0.0 does not allow letters in UIDs  the standard allows only 0-9 and . separator
          // leading zeros are also not allowed
            Study1UID    = "9999.1111.1" ;
            Study2UID    = "9999.1111.2" ;
            Study3UID    =  Study2UID ;
            
            Series1UID   = "1111.1111.1" ;
            Series2UID   = "1111.1111.2" ;
            Series3UID   = "1111.1111.3" ;
            
            Instance1UID = "2222.1111.1" ;
            Instance2UID = "2222.1111.2" ;
            Instance3UID = "2222.1111.3" ;

            SOPClass1UID = "3333.1111.1" ;
        }
        
        public static string GetBaseFolder ( ) 
        {
            string baseFolder = System.AppDomain.CurrentDomain.BaseDirectory ;        

            return new DirectoryInfo ( baseFolder ).Parent.Parent.Parent.Parent.FullName ;
        }

        public static string GetTestDataFolder (string testDataFolder, bool create = false )
        {
            string   folderPath  = Path.Combine (GetBaseFolder ( ), TestFolderName, testDataFolder ) ;

            if ( create )
            {
                Directory.CreateDirectory ( folderPath ) ;
            }

            return folderPath ;
        }

        public static string GetSampleImagesFolder ( ) 
        { 
            return Path.Combine ( DicomHelpers.GetBaseFolder ( ), "resources", "sampleimages" ) ;
        }

        public static string   TestFolderName = "Test_Data" ;

        public fo.DicomDataset GetDicomDataset ( uint dsNumber) 
        {
            uint testDsCase = dsNumber % 3 ;
            fo.DicomDataset testDs = new fo.DicomDataset ( ) ;


            switch ( testDsCase )
            {
                case 0:
                {
                    return GetTemplateDataset ( ).CopyTo ( testDs ) ;
                }

                case 1:
                {

                    testDs = GetTemplateDataset().CopyTo ( testDs ) ;

                    testDs .AddOrUpdate ( fo.DicomTag.StudyInstanceUID, Study2UID );
                    testDs .AddOrUpdate ( fo.DicomTag.SeriesInstanceUID, Series2UID );
                    testDs .AddOrUpdate ( fo.DicomTag.SOPInstanceUID, Instance2UID );
                }
                break ;

                case 2:
                {
                    testDs = GetTemplateDataset().CopyTo ( testDs ) ;

                    testDs.AddOrUpdate ( fo.DicomTag.StudyInstanceUID, Study3UID );
                    testDs.AddOrUpdate ( fo.DicomTag.SeriesInstanceUID, Series3UID );
                    testDs.AddOrUpdate  ( fo.DicomTag.SOPInstanceUID, Instance3UID ) ;
                }
                break ;

                default:
                {
                    throw new IndexOutOfRangeException ( "Test dataset case not implemented") ;
                }
            }

            return testDs ;
        }
        
        public string Study1UID
        {
            get; set;
        }

        public string Study2UID
        {
            get; set;
        }

        public string Study3UID
        {
            get; set;
        }

        public string Series1UID
        {
            get; set;
        }

        public string Series2UID
        {
            get; set;
        }

        public string Series3UID
        {
            get; set;
        }

        public string Instance1UID
        {
            get; set;
        }

        public string Instance2UID
        {
            get; set;
        }

        public string Instance3UID
        {
            get; set;
        }
        public string SOPClass1UID
        {
            get; set;
        }


        public fo.DicomDataset GetQueryDataset ( ) 
        {
            var ds = new fo.DicomDataset ( ) ;


            ds.Add<string> ( fo.DicomTag.PatientID,(string) null) ;
            ds.Add<string>( fo.DicomTag.PatientName, (string)null);
            ds.Add<string>( fo.DicomTag.StudyInstanceUID, (string)null);
            ds.Add<string>( fo.DicomTag.StudyID, (string)null);
            ds.Add<string>(fo.DicomTag.StudyDate, (string)null);
            ds.Add<string>( fo.DicomTag.AccessionNumber, (string)null);
            ds.Add<string>(fo.DicomTag.StudyDescription, (string)null);
            ds.Add<string>( fo.DicomTag.SeriesInstanceUID, (string)null);
            ds.Add<string>( fo.DicomTag.SeriesNumber, (string)null);
            ds.Add<string>( fo.DicomTag.Modality, (string)null);
            ds.Add<string>( fo.DicomTag.SOPInstanceUID, (string)null);
            ds.Add<string>( fo.DicomTag.SOPClassUID, (string)null);
            ds.Add<string>( fo.DicomTag.InstanceNumber, (string)null);

            ds.Add<int>(fo.DicomTag.NumberOfFrames);
            ds.Add<ushort>(fo.DicomTag.BitsAllocated);
            ds.Add<ushort>(fo.DicomTag.Rows);
            ds.Add<ushort>(fo.DicomTag.Columns);

            return ds ;
        }

        private fo.DicomDataset GetTemplateDataset ( ) 
        {
            var ds = new fo.DicomDataset ( ) ;


            ds.Add ( fo.DicomTag.PatientID, "test-pid") ;
            ds.Add ( fo.DicomTag.PatientName, "test^patient name" );
            ds.Add ( fo.DicomTag.StudyInstanceUID, Study1UID );
            ds.Add ( fo.DicomTag.StudyID, "test-studyid" );
            ds.Add (fo.DicomTag.StudyDate, "20181112");
            ds.Add ( fo.DicomTag.AccessionNumber, "test-accession" );
            ds.Add (fo.DicomTag.StudyDescription, "test-description");
            ds.Add ( fo.DicomTag.SeriesInstanceUID, Series1UID );
            ds.Add ( fo.DicomTag.SeriesNumber, 1 );
            ds.Add ( fo.DicomTag.Modality, "XA" );
            ds.Add ( fo.DicomTag.SOPInstanceUID, Instance1UID );
            ds.Add ( fo.DicomTag.SOPClassUID, SOPClass1UID);
            ds.Add ( fo.DicomTag.InstanceNumber, 1 );

            ds.Add(fo.DicomTag.NumberOfFrames, 1);
            ds.Add(fo.DicomTag.BitsAllocated, (ushort)16);
            ds.Add(fo.DicomTag.Rows, (ushort)255);
            ds.Add(fo.DicomTag.Columns, (ushort)512);

            return ds ;
        }

    }
}
