using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary.ExceptionHandling;
using RestSharp;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.IO;
using Response;
using System.Text.RegularExpressions;
using System.Web;
using SAPmyDataService.Enumerators;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Data.Odbc;

namespace SAPmyDataService.BusinessLayer
{
    public class myDataMethods
    {
        #region Public Properties
        public List<BoDocument> ListDocuments { get; set; }
        public SAPbobsCOM.Company CompanyConnection { get; set; }
        public List<BoDocument> ListDocumentsCancel { get; set; }
        public SAPbobsCOM.Company CompanyConnectionCancel { get; set; }
        public int returnsRows { get; set; }

        #endregion

        #region Private Properties
        private RestClient Client { get; set; }
        private RestRequest Request { get; set; }
        #endregion

        public myDataMethods()
        {
            this.ListDocuments = new List<BoDocument>();
        }

        #region Public Methods
        public int LoadnCreate(Enumerators.ot_Object _enType)
        {
            int iRetVal = 0;
            try
            {
                LoadnCreateClass oLoadnCreate = new LoadnCreateClass();
                oLoadnCreate.CompanyConnection = this.CompanyConnection;
                oLoadnCreate.returnsRows = this.returnsRows;
                iRetVal = oLoadnCreate.Exec(_enType);
                this.returnsRows = oLoadnCreate.returnsRows;

                //if (iRetVal == 1) //έγινε σχόλιο γτ τρέχω ΜΟΝΟ αυτά που είναι πετυχημένα!
                //{
                this.ListDocuments = new List<BoDocument>();
                this.ListDocuments = oLoadnCreate.ListDocuments;
                //}
            }
            catch (Exception ex)
            {
                var a = new Logging("myDataMethods.LoadnCreate", ex);
            }
            return iRetVal;
        }

        public int LoadnCreateCancel(Enumerators.ot_Object _enType)
        {
            int iRetVal = 0;
            try
            {
                LoadnCreateClassCancel oLoadnCreateCancel = new LoadnCreateClassCancel();
                oLoadnCreateCancel.CompanyConnectionCancel = this.CompanyConnection;
                oLoadnCreateCancel.returnsRows = this.returnsRows;
                iRetVal = oLoadnCreateCancel.Exec(_enType);
                this.returnsRows = oLoadnCreateCancel.returnsRows;
                //if (iRetVal == 1) //έγινε σχόλιο γτ τρέχω ΜΟΝΟ αυτά που είναι πετυχημένα!
                //{
                this.ListDocumentsCancel = new List<BoDocument>();
                this.ListDocumentsCancel = oLoadnCreateCancel.ListDocumentsCancel;
                //}
            }
            catch (Exception ex)
            {
                var a = new Logging("myDataMethods.LoadnCreateCancel", ex);
            }
            return iRetVal;
        }

        public int Send(ot_Object _enType)
        {
            int iRetVal = 0;
            try
            {
                CommonLibrary.Ini.IniFile ini = new CommonLibrary.Ini.IniFile("C:\\Program Files\\SAP\\SAPmyDataService\\ConfParams.ini");
                int updateMark = int.Parse(ini.IniReadValue("Default", "UPDATE_MARK").ToString());
                for (int i = 0; i < this.ListDocuments.Count; i++)
                {
                    if (ListDocuments[i].DocumentStatus == DocumentPrepared.p_Success)
                    {
                        this.Send2AADE(this.ListDocuments[i]);
                        if (!string.IsNullOrEmpty(ListDocuments[i].MARK) && ListDocuments[i].StatusCode.Equals("Success") && updateMark == 1)
                        {
                            int iTempResult = this.UpdateSAPDocuments(this.ListDocuments[i]);
                            if (this.ListDocuments[i].Result == Enumerators.SAPResult.sr_Success && iTempResult == 1)
                            {
                                this.UpdateDocumentSETSAPUpdate(this.ListDocuments[i]);

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var a = new Logging("myDataMethods.Send", ex);
            }
            return iRetVal;
        }

        /// <summary>
        /// Ενημέρωση Παραστατικού SAP Business One
        /// </summary>
        /// <param name="_oDocument"> To Παραστατικό που θα Ενημερωθεί</param>
        /// <returns>1 For Success, 0 For Failure</returns>
        private int UpdateSAPDocuments(BoDocument _oDocument)
        {
            int iRetVal = 0;
            string sSQL = "";
            string sDocumentTypeDsc = "";
            int iResult = 0;
            try
            {
                if (CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    switch (_oDocument.ObjType)
                    {
                        case "13":
                            sDocumentTypeDsc = "Sales Invoice";
                            sSQL = "UPDATE OINV SET \"U_Mark\" = N'" + _oDocument.MARK + "', \"U_UID\" = N'" + _oDocument.UID + "',\"U_ClassMark\"=N'" + "' WHERE 1=1 AND \"DocEntry\" = " + _oDocument.DocEntry + "";
                            break;
                        case "14":
                            sDocumentTypeDsc = "Sales Credit Note";
                            sSQL = "UPDATE ORIN SET \"U_Mark\" = N'" + _oDocument.MARK + "', \"U_UID\" = N'" + _oDocument.UID + "',\"U_ClassMark\"=N'" + "' WHERE 1=1 AND \"DocEntry\" = " + _oDocument.DocEntry + "";
                            break;
                        case "18":
                            sDocumentTypeDsc = "Purchase Invoice";
                            sSQL = "UPDATE OPCH SET \"U_Mark\" = N'" + _oDocument.MARK + "', \"U_UID\" = N'" + _oDocument.UID + "',\"U_ClassMark\"=N'" + _oDocument.ClassMark + "' WHERE 1=1 AND \"DocEntry\" = " + _oDocument.DocEntry + "";
                            break;
                        case "19":
                            sDocumentTypeDsc = "Purchase Credit Note";
                            sSQL = "UPDATE ORPC SET \"U_Mark\" = N'" + _oDocument.MARK + "', \"U_UID\" = N'" + _oDocument.UID + "',\"U_ClassMark\"=N'" + _oDocument.ClassMark + "' WHERE 1=1 AND \"DocEntry\" = " + _oDocument.DocEntry + "";
                            break;
                        case "30":
                            sDocumentTypeDsc = "Journal Entry";
                            sSQL = "UPDATE OJDT SET \"U_Mark\" = N'" + _oDocument.MARK + "', \"U_UID\" = N'" + _oDocument.UID + "' WHERE 1=1 AND \"TransId\" = " + _oDocument.DocEntry + "";
                            break;
                    }
                }
                else
                {
                    switch (_oDocument.ObjType)
                    {
                        case "13":
                            sDocumentTypeDsc = "Sales Invoice";
                            sSQL = "UPDATE OINV SET U_Mark = N'" + _oDocument.MARK + "', U_UID = N'" + _oDocument.UID + "',U_ClassMark=N'" + "' WHERE 1=1 AND DocEntry = " + _oDocument.DocEntry + "";
                            break;
                        case "14":
                            sDocumentTypeDsc = "Sales Credit Note";
                            sSQL = "UPDATE ORIN SET U_Mark = N'" + _oDocument.MARK + "', U_UID = N'" + _oDocument.UID + "',U_ClassMark=N'" + "'WHERE 1=1 AND DocEntry = " + _oDocument.DocEntry + "";
                            break;
                        case "18":
                            sDocumentTypeDsc = "Purchase Invoice";
                            sSQL = "UPDATE OPCH SET U_Mark = N'" + _oDocument.MARK + "', U_UID = N'" + _oDocument.UID + "',U_ClassMark=N'" + _oDocument.ClassMark + "' WHERE 1=1 AND DocEntry = " + _oDocument.DocEntry + "";
                            break;
                        case "19":
                            sDocumentTypeDsc = "Purchase Credit Note";
                            sSQL = "UPDATE ORPC SET U_Mark = N'" + _oDocument.MARK + "', U_UID = N'" + _oDocument.UID + "',U_ClassMark=N'" + _oDocument.ClassMark + "' WHERE 1=1 AND DocEntry = " + _oDocument.DocEntry + "";
                            break;
                        case "30":
                            sDocumentTypeDsc = "Journal Entry";
                            sSQL = "UPDATE OJDT SET U_Mark = N'" + _oDocument.MARK + "', U_UID = N'" + _oDocument.UID + "'WHERE 1=1 AND TransId = " + _oDocument.DocEntry + "";
                            break;
                    }

                }

                SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, CompanyConnection);
                if (oRS != null)
                {
                    _oDocument.Result = Enumerators.SAPResult.sr_Success;
                    iResult++;

                }
                int objtp = int.Parse(_oDocument.ObjType);
                if (iResult == 1 && (objtp == 13 || objtp == 14 || ((objtp == 18 || objtp == 19) && _oDocument.DocumentType == Enumerators.DocumentType.p_EU_TX)))
                {
                    iResult += this.UpdateQR(_oDocument, CompanyConnection);
                }
                else
                {
                    iResult++;
                }
                if (iResult == 2)
                {
                    iRetVal++;
                }

                //Console.WriteLine("" + sDocumentTypeDsc + " " + _oDocument.DocNum + " Successfully Updated!");
            }
            catch (Exception ex)
            {
                //Console.WriteLine("" + sDocumentTypeDsc + " " + _oDocument.DocNum + " Cannot be Updated!");
                Logging.WriteToLog("sSQL=" + sSQL, Logging.LogStatus.RET_VAL);
                _oDocument.Result = Enumerators.SAPResult.sr_Failure;
                var a = new Logging("myDataMethods.UpdateSAPDocuments", ex);
            }
            return iRetVal;
        }

        private int UpdateQR(BoDocument _oDocument, SAPbobsCOM.Company CompanyConnection)
        {
            int iRetVal = 0;
            string sSQL = "";
            try
            {
                SAPbobsCOM.Documents oDIDocument = null;

                if (_oDocument.ObjType == "13")
                {
                    oDIDocument = (SAPbobsCOM.Documents)CompanyConnection.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);
                }
                else if (_oDocument.ObjType == "14")
                {
                    oDIDocument = (SAPbobsCOM.Documents)CompanyConnection.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes);
                }
                else if (_oDocument.ObjType == "18")
                {
                    oDIDocument = (SAPbobsCOM.Documents)CompanyConnection.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseInvoices);
                }
                else if (_oDocument.ObjType == "18")
                {
                    oDIDocument = (SAPbobsCOM.Documents)CompanyConnection.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseCreditNotes);
                }
                string sDocEntry = _oDocument.DocEntry;//_oDocument.GetDocEntry();
                bool bLoad = oDIDocument.GetByKey(int.Parse(sDocEntry));

                if (bLoad == true)
                {
                    oDIDocument.CreateQRCodeFrom = _oDocument.QR;

                    int iDIResult = oDIDocument.Update();

                    if (iDIResult == 0)
                    {
                        iRetVal++;
                    }
                    else
                    {
                        int nErr;
                        string sErrMsg;
                        Connection.oCompany.GetLastError(out nErr, out sErrMsg);

                        Console.WriteLine(nErr.ToString() + " / " + sErrMsg);
                        Logging.WriteToLog("DI ERROR on Document with ObjType=" + _oDocument.ObjType + " and DocEntry=" + _oDocument.DocEntry + " | " + nErr.ToString() + " / " + sErrMsg, Logging.LogStatus.RET_VAL);
                    }
                }
                else
                {
                    Console.WriteLine("Δεν ήταν Δυνατή η Φόρτωση του Παραστατικού");
                }
            }
            catch (Exception ex)
            {
                Logging.WriteToLog("sSQL=" + sSQL, Logging.LogStatus.RET_VAL);
                var a = new Logging("myDataMethods.UpdateQR", ex);
            }
            return iRetVal;
        }

        public int UpdateDocumentSETSAPUpdate(BusinessLayer.BoDocument _oDocument)
        {
            int iRetVal = 0;
            string sSQL = "";
            try
            {
                if (_oDocument.isExpense == 0)
                {
                    if (CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        sSQL = "CALL DOCUMENTS_UPDATE_SET_SAP_UPDATED('" + _oDocument.DocumentAA + "')";
                    }
                    else
                    {
                        sSQL = "exec DOCUMENTS_UPDATE_SET_SAP_UPDATED '" + _oDocument.DocumentAA + "'";
                    }
                }
                else if (_oDocument.isExpense == 1)
                {
                    if (CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        sSQL = "CALL EXPENSES_UPDATE_SET_SAP_UPDATED('" + _oDocument.DocumentAA + "')";
                    }
                    else
                    {
                        sSQL = "exec EXPENSES_UPDATE_SET_SAP_UPDATED '" + _oDocument.DocumentAA + "' ";
                    }

                }
                SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, CompanyConnection);
                if (oRS != null)
                {
                    iRetVal++;
                }
                else
                {
                    Logging.WriteToLog("Failed to Update Document with AA=" + _oDocument.DocumentAA + " sSQL=" + sSQL, Logging.LogStatus.RET_VAL);
                }
            }
            catch (Exception ex)
            {
                Logging.WriteToLog("sSQL=" + sSQL, Logging.LogStatus.RET_VAL);
                var a = new Logging("BoDAL.UpdateDocumentSETSAPUpdate", ex);
            }
            return iRetVal;
        }
        public int CancelInvoice()
        {
            int iRetVal = 0;
            try
            {
                int iResult = 0;
                int iSuccess = this.ListDocumentsCancel.Count;

                for (int i = 0; i < this.ListDocumentsCancel.Count; i++)
                {
                    iResult += this.Cancel(this.ListDocumentsCancel[i]);
                }

                if (iRetVal == iSuccess)
                {
                    iRetVal++;
                }
            }
            catch (Exception ex)
            {
                var a = new Logging("myDataMethods.CancelInvoice", ex);
            }
            return iRetVal;
        }
        #endregion

        #region Private Methods
        private int Send2AADE(BoDocument _oDocument)
        {
            int iRetVal = 0;
            try
            {
                string sFileLocation = "C:\\Program Files\\SAP\\SAPmyDataService\\ConfParams.ini";
                CommonLibrary.Ini.IniFile ini = new CommonLibrary.Ini.IniFile(sFileLocation);
                string xmlPath = ini.IniReadValue("Default", "XML_PATH");
                string sProxy = ini.IniReadValue("Default", "PROXY_SERVER");
                string sEndPoint = "";
                string sUser = "";
                string sSubscription = "";
                _oDocument.StatusCode = "";
                System.Xml.Serialization.XmlSerializer oXML = null;
                MemoryStream ms = new MemoryStream();
                if (_oDocument.DocumentType == Enumerators.DocumentType.p_Income) //είναι expense και δεν έχει mark => είναι δικό μας document EU/TX
                {
                    sEndPoint = ini.IniReadValue("Default", "ENDPOINT_SEND_INVOICES");
                    sUser = ini.IniReadValue("Default", "AADE_USER_ID");
                    sSubscription = ini.IniReadValue("Default", "AADE_SUBSCRIPTION_KEY");
                    oXML = new System.Xml.Serialization.XmlSerializer(typeof(InvoicesDoc));
                    oXML.Serialize(ms, _oDocument.AADEDocument);
                }
                else if ((_oDocument.DocumentType == Enumerators.DocumentType.p_Matched || _oDocument.DocumentType == Enumerators.DocumentType.p_Deviation || _oDocument.DocumentType == Enumerators.DocumentType.p_Reject) && (_oDocument.ObjType.Equals("18") || _oDocument.ObjType.Equals("19")))
                {
                    sEndPoint = ini.IniReadValue("Default", "ENDPOINT_SEND_EXPENSES_CLASSIFICATIONS");
                    sUser = ini.IniReadValue("Default", "AADE_USER_ID_EXPENSES");
                    sSubscription = ini.IniReadValue("Default", "AADE_SUBSCRIPTION_KEY_EXPENSES");
                    oXML = new System.Xml.Serialization.XmlSerializer(typeof(ExpensesClassificationsDoc));
                    oXML.Serialize(ms, _oDocument.AADEMatchingDocument);

                }
                else if ((_oDocument.DocumentType == Enumerators.DocumentType.p_Matched || _oDocument.DocumentType == Enumerators.DocumentType.p_Deviation || _oDocument.DocumentType == Enumerators.DocumentType.p_Reject) && (_oDocument.ObjType.Equals("13") || _oDocument.ObjType.Equals("14")))
                {
                    sEndPoint = ini.IniReadValue("Default", "ENDPOINT_SEND_INCOME_CLASSIFICATIONS");
                    sUser = ini.IniReadValue("Default", "AADE_USER_ID_EXPENSES");
                    sSubscription = ini.IniReadValue("Default", "AADE_SUBSCRIPTION_KEY_EXPENSES");
                    oXML = new System.Xml.Serialization.XmlSerializer(typeof(IncomeClassificationsDoc));
                    oXML.Serialize(ms, _oDocument.AADEMatchingDocumentIncome);

                }
                else if (_oDocument.DocumentType == Enumerators.DocumentType.p_EU_TX && !_oDocument.ObjType.Equals("444")) //444 είναι για αποκλίσεις
                {
                    sEndPoint = ini.IniReadValue("Default", "ENDPOINT_SEND_INVOICES");
                    sUser = ini.IniReadValue("Default", "AADE_USER_ID");
                    sSubscription = ini.IniReadValue("Default", "AADE_SUBSCRIPTION_KEY");
                    oXML = new System.Xml.Serialization.XmlSerializer(typeof(InvoicesDoc));
                    oXML.Serialize(ms, _oDocument.AADEDocument);
                }
                else if (_oDocument.DocumentType == Enumerators.DocumentType.p_EU_TX && _oDocument.ObjType.Equals("444"))
                {
                    sEndPoint = ini.IniReadValue("Default", "ENDPOINT_SEND_INVOICES");
                    sUser = ini.IniReadValue("Default", "AADE_USER_ID_EXPENSES");
                    sSubscription = ini.IniReadValue("Default", "AADE_SUBSCRIPTION_KEY_EXPENSES");
                    oXML = new System.Xml.Serialization.XmlSerializer(typeof(InvoicesDoc));
                    oXML.Serialize(ms, _oDocument.AADEDocument);
                }


                //string sEndPoint = "https://mydata-dev.azure-api.net/SendInvoices";

                if (_oDocument.useNewMethod == 1)
                {
                    //this.Request.AddHeader("postPerInvoice", "true");

                    //this.Request.AddParameter("postPerInvoice", true, ParameterType.HttpHeader);
                    sEndPoint += "?postPerInvoice=true";
                }


                this.Client = new RestClient(sEndPoint);
                this.Client.Timeout = -1;
                this.Request = new RestRequest(Method.POST);
                this.Request.AddHeader("aade-user-id", sUser);
                this.Request.AddHeader("ocp-apim-subscription-key", sSubscription);



                //object q = _oDocument.AADEDocument;
                //System.Xml.Serialization.XmlSerializer oXML = null;
                //MemoryStream ms = new MemoryStream();
                //oXML = new System.Xml.Serialization.XmlSerializer(typeof(InvoicesDoc));
                //oXML.Serialize(ms, _oDocument.AADEDocument);
                ms.Position = 0;

                StreamReader SR = new StreamReader(ms);
                string sBody = SR.ReadToEnd();




                this.Request.AddParameter("application/text", sBody, ParameterType.RequestBody);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                if (!string.IsNullOrEmpty(sProxy))
                {
                    WebProxy proxy = new WebProxy(sProxy, true);
                    proxy.UseDefaultCredentials = true;
                    WebRequest.DefaultWebProxy = proxy;
                }

                string sPath = xmlPath + "\\2AADE\\" + _oDocument.ObjType + "_" + _oDocument.DocEntry + "_" + _oDocument.DocNum + ".xml";

                using (StreamWriter sw = File.CreateText(sPath))
                {
                    sw.WriteLine(sBody);
                }

                IRestResponse oResponse = this.Client.Execute(this.Request);

                if (oResponse.StatusCode == HttpStatusCode.OK)
                {
                    sPath = xmlPath + "\\" + _oDocument.ObjType + "_" + _oDocument.DocEntry + "_" + _oDocument.DocNum + ".xml";
                    string sXML = "";

                    using (StreamWriter sw = File.CreateText(sPath))
                    {
                        sXML = oResponse.Content;
                        sXML = sXML.Substring(1, sXML.Length - 1);
                        sXML = sXML.Replace("\\r\\n", "");
                        sXML = sXML.Replace("</ResponseDoc>\"", "</ResponseDoc>");
                        sXML = sXML.Replace("\\", "").Replace("\"", "\"");
                        sw.WriteLine(sXML);
                    }


                    Logging.WriteToLog("myDataMethods.AddResponse", Logging.LogStatus.START);
                    this.AddResponse(_oDocument, sXML, 0);
                    Logging.WriteToLog("myDataMethods.AddResponse", Logging.LogStatus.END);

                    System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(ResponseDoc));

                    using (StreamReader sr = new StreamReader(sPath))
                    {
                        ResponseDoc oReply = (ResponseDoc)ser.Deserialize(sr);

                        Logging.WriteToLog("myDataMethods.UpdateDocument", Logging.LogStatus.START);
                        this.UpdateDocument(_oDocument, oReply, false);
                        Logging.WriteToLog("myDataMethods.UpdateDocument", Logging.LogStatus.END);
                    }
                }
                else
                {
                    _oDocument.StatusCode = "Failure";
                    Logging.WriteToLog("Processing Document:" + _oDocument.ObjType + " / " + _oDocument.DocNum + "", Logging.LogStatus.RET_VAL);
                    Logging.WriteToLog("Error Contacting EndPoint:" + oResponse.StatusCode + "/" + oResponse.StatusDescription, Logging.LogStatus.ERROR);
                }
            }
            catch (Exception ex)
            {
                var a = new Logging("myDataMethods.Send2AADE", ex);
            }
            return iRetVal;
        }
        private int Cancel(BoDocument _oDocument)
        {
            int iRetVal = 0;
            try
            {
                string sFileLocation = "C:\\Program Files\\SAP\\SAPmyDataService\\ConfParams.ini";
                CommonLibrary.Ini.IniFile ini = new CommonLibrary.Ini.IniFile(sFileLocation);
                string sProxy = ini.IniReadValue("Default", "PROXY_SERVER");
                string xmlPath = ini.IniReadValue("Default", "XML_PATH");

                string sEndPoint = ini.IniReadValue("Default", "ENDPOINT_CANCEL_INVOICE");
                string sUser = "";
                string sSubscription = "";

                /* if (_oDocument.isExpense == 1)
                 {
                     sUser = ini.IniReadValue("Default", "AADE_USER_ID_EXPENSES");
                     sSubscription = ini.IniReadValue("Default", "AADE_SUBSCRIPTION_KEY_EXPENSES");
                 }
                 else
                 {
                     sUser = ini.IniReadValue("Default", "AADE_USER_ID");
                     sSubscription = ini.IniReadValue("Default", "AADE_SUBSCRIPTION_KEY");

                 }*/

                sUser = ini.IniReadValue("Default", "AADE_USER_ID");
                sSubscription = ini.IniReadValue("Default", "AADE_SUBSCRIPTION_KEY");


                var queryString = HttpUtility.ParseQueryString(string.Empty);
                queryString["mark"] = "" + _oDocument.MARK + "";

                //sEndPoint = sEndPoint.Replace("#MARK", _oDocument.MARK);

                sEndPoint = sEndPoint + queryString;

                this.Client = new RestClient(sEndPoint);
                this.Client.Timeout = -1;
                this.Request = new RestRequest(Method.POST);
                this.Request.AddHeader("aade-user-id", sUser);
                this.Request.AddHeader("ocp-apim-subscription-key", sSubscription);
                //this.Request.AddParameter("mark", _oDocument.MARK);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                if (!string.IsNullOrEmpty(sProxy))
                {
                    WebProxy proxy = new WebProxy(sProxy, true);
                    proxy.UseDefaultCredentials = true;
                    WebRequest.DefaultWebProxy = proxy;
                }

                IRestResponse oResponse = this.Client.Execute(this.Request);

                if (oResponse.StatusCode == HttpStatusCode.OK)
                {
                    string sPath = xmlPath + "\\" + _oDocument.MARK + ".xml";
                    string sXML = "";

                    using (StreamWriter sw = File.CreateText(sPath))
                    {
                        sXML = oResponse.Content;
                        sXML = sXML.Substring(1, sXML.Length - 1);
                        sXML = sXML.Replace("\\r\\n", "");
                        sXML = sXML.Replace("</ResponseDoc>\"", "</ResponseDoc>");
                        sXML = sXML.Replace("\\", "").Replace("\"", "\"");
                        sw.WriteLine(sXML);
                    }

                    Logging.WriteToLog("myDataMethods.AddResponse", Logging.LogStatus.START);
                    this.AddResponse(_oDocument, sXML, 1);
                    Logging.WriteToLog("myDataMethods.AddResponse", Logging.LogStatus.END);

                    System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(ResponseDoc));

                    using (StreamReader sr = new StreamReader(sPath))
                    {
                        ResponseDoc oReply = (ResponseDoc)ser.Deserialize(sr);

                        Logging.WriteToLog("myDataMethods.UpdateDocument", Logging.LogStatus.START);
                        this.UpdateDocument(_oDocument, oReply, true);
                        Logging.WriteToLog("myDataMethods.UpdateDocument", Logging.LogStatus.END);
                    }
                }
            }
            catch (Exception ex)
            {
                var a = new Logging("myDataMethods.Cancel", ex);
            }
            return iRetVal;
        }

        private int AddResponse(BoDocument _oDocument, string _sXML, int _iCancel)
        {
            int iRetVal = 0;
            try
            {
                DAL.BoUpdateDB oLog = new DAL.BoUpdateDB();
                oLog.DocumentAA = _oDocument.DocumentAA;
                oLog.DocEntry = _oDocument.DocEntry;
                oLog.DocNum = _oDocument.DocNum;
                oLog.ObjType = _oDocument.ObjType;
                oLog.XMLReply = _sXML;
                oLog.Company = _oDocument.CompanyDB;
                oLog.Cancel = _iCancel;
                iRetVal = oLog.AddResponse(CompanyConnection);
            }
            catch (Exception ex)
            {
                var a = new Logging("myDataMethods.AddResponse", ex);
            }
            return iRetVal;
        }
        private int UpdateDocument(BoDocument _oDocument, ResponseDoc _oReply, bool _bCancel)
        {
            int iRetVal = 0;
            try
            {
                string sStatusCode = _oReply.response[0].statusCode;
                string sTableName = "";
                DAL.BoUpdateDB oLog = new DAL.BoUpdateDB();
                oLog.DocumentAA = _oDocument.DocumentAA;
                oLog.DocEntry = _oDocument.DocEntry;
                oLog.ObjType = _oDocument.ObjType;
                oLog.Result = sStatusCode;
                oLog.Company = _oDocument.CompanyDB;
                oLog.type = 0;
                oLog.isChecked = -1;
                if (_oDocument.DocumentType == Enumerators.DocumentType.p_EU_TX)
                {
                    sTableName = "EXPENSES";
                    oLog.type = 1;
                }
                else if (_oDocument.DocumentType == Enumerators.DocumentType.p_Income)
                {
                    sTableName = "DOCUMENTS";

                }
                else
                {
                    sTableName = "EXPENSES";

                }


                if (_bCancel == false)              //original upload to aade
                {
                    if (sStatusCode == "Success")
                    {
                        if (_oDocument.DocumentType == Enumerators.DocumentType.p_Matched)
                        {
                            _oDocument.MARK = _oReply.response[0].Items[0].ToString();
                            _oDocument.ClassMark = _oReply.response[0].Items[1].ToString();
                            oLog.MARK = _oDocument.ClassMark;
                        }
                        else
                        {
                            _oDocument.UID = _oReply.response[0].Items[0].ToString();
                            _oDocument.MARK = _oReply.response[0].Items[1].ToString();
                            _oDocument.ClassMark = "";
                        }
                        _oDocument.StatusCode = sStatusCode;
                        oLog.UID = _oReply.response[0].Items[0].ToString();
                        oLog.MARK = _oReply.response[0].Items[1].ToString();
                        oLog.CANCELLED_MARK = null;
                        oLog.isChecked = 2;

                        List<ItemsChoiceType> fieldsList = _oReply.response[0].ItemsElementName.ToList();
                        if (fieldsList.Contains(ItemsChoiceType.qrUrl))
                        {
                            _oDocument.QR = _oReply.response[0].Items[2].ToString();
                        }
                        else
                        {
                            _oDocument.QR = "";
                        }
                        oLog.QR = _oDocument.QR;
                    }
                    else
                    {
                        _oDocument.MARK = "IN_PROCESS";
                        _oDocument.UID = "";
                        _oDocument.ClassMark = "";
                        _oDocument.QR = "";
                        _oDocument.StatusCode = sStatusCode;
                        if (_oReply.response[0].ItemsElementName[0] == ItemsChoiceType.errors)
                        {
                            ResponseTypeErrors oError = (ResponseTypeErrors)_oReply.response[0].Items[0];
                            oLog.ErrorCode = oError.error[0].code.ToString();
                            oLog.ErrorDescr = oError.error[0].message.ToString();
                            oLog.isChecked = 1;
                        }
                        oLog.MARK = oLog.UID = oLog.CANCELLED_MARK = oLog.QR = null;
                    }
                }
                else                          //cancelled 
                {
                    if (sStatusCode == "Success")
                    {
                        oLog.CANCELLED_MARK = _oReply.response[0].Items[0].ToString();
                        oLog.MARK = null;
                        oLog.UID = null;
                        oLog.QR = null;
                        oLog.isChecked = 2;
                    }
                    else
                    {
                        if (_oReply.response[0].ItemsElementName[0] == ItemsChoiceType.errors)
                        {
                            ResponseTypeErrors oError = (ResponseTypeErrors)_oReply.response[0].Items[0];
                            oLog.ErrorCode = oError.error[0].code.ToString();
                            oLog.ErrorDescr = oError.error[0].message.ToString();
                        }
                        oLog.isChecked = 2;
                    }
                }

                if (_bCancel == true)
                {
                    oLog.UpdateDocument4Cancel(sTableName, CompanyConnection);
                }
                else
                {
                    oLog.UpdateDocument(sTableName, CompanyConnection);
                }

                #region old
                /*if (sStatusCode == "Success")
                {
                    if (_bCancel == true)
                    {
                        oLog.CANCELLED_MARK = _oReply.response[0].Items[1].ToString();
                        oLog.MARK = null;

                    }
                    else
                    {
                        oLog.MARK = _oReply.response[0].Items[1].ToString();
                        oLog.CANCELLED_MARK = null;
                    }
                    //Να δω αν χρειάζεται στα cancelled
                    oLog.UID = _oReply.response[0].Items[0].ToString();
                    oLog.UpdateDocument();
                }
                else
                {
                    if (_oReply.response[0].ItemsElementName[0] == ItemsChoiceType.errors)
                    {
                        ResponseTypeErrors oError = (ResponseTypeErrors)_oReply.response[0].Items[0];
                        oLog.ErrorCode = oError.error[0].code.ToString();
                        oLog.ErrorDescr = oError.error[0].message.ToString();
                    }

                    oLog.MARK = null;
                    oLog.UID = null;

                    if (_bCancel == true) //Αυτό γτ Δεν θέλω να χαθεί το MARK & το UID που είχε πάρει
                    {
                        oLog.MARK = _oDocument.MARK;
                        oLog.UID = _oDocument.UID;
                    }
                    else
                    {
                        oLog.MARK = "";
                        oLog.UID = "";
                    }

                    oLog.UpdateDocument();
                }*/
                #endregion
            }
            catch (Exception ex)
            {
                var a = new Logging("myDataMethods.UpdateDocument", ex);
            }
            return iRetVal;
        }
        #endregion

        #region Nested Classes
        internal class LoadnCreateClass
        {
            #region Public Properties
            public List<BoDocument> ListDocuments { get; set; }
            public SAPbobsCOM.Company CompanyConnection { get; set; }
            public int returnsRows { get; set; }

            #endregion

            #region Private Properties

            #endregion

            public LoadnCreateClass()
            {
                this.ListDocuments = new List<BoDocument>();
            }

            #region Private Methods
            private int LoadDocumentsProcess()
            {
                string sSQL = "";
                int iRetVal = 0;
                try
                {

                    this.ListDocuments = new List<BoDocument>();
                    BoDocument oDocument = null;

                    if (CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_2_PROCESS WHERE 1=1 ORDER BY AA DESC";
                    }
                    else
                    {
                        sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_2_PROCESS WHERE 1=1 ORDER BY AA DESC";
                    }

                    SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, CompanyConnection);

                    this.returnsRows = oRS.RecordCount;
                    while (oRS.EoF == false)
                    {
                        oDocument = new BoDocument();
                        oDocument.DocumentAA = oRS.Fields.Item("AA").Value.ToString();
                        oDocument.CompanyDB = oRS.Fields.Item("COMPANY_DB").Value.ToString();
                        oDocument.ObjType = oRS.Fields.Item("OBJTYPE").Value.ToString();
                        oDocument.DocEntry = oRS.Fields.Item("DOCENTRY").Value.ToString();
                        oDocument.DocNum = oRS.Fields.Item("DOCNUM").Value.ToString();
                        oDocument.isExpense = int.Parse(oRS.Fields.Item("ISEXPENSE").Value.ToString());
                        oDocument.updateExpenses = int.Parse(oRS.Fields.Item("ISEXPENSE").Value.ToString());
                        oDocument.reject_deviation = oRS.Fields.Item("REJECT_DEVIATION").Value.ToString();
                        oDocument.MARK = oRS.Fields.Item("MARK").Value.ToString();
                        oDocument.useNewMethod = int.Parse(oRS.Fields.Item("useNewMethod").Value.ToString());
                        oDocument.CounterPart_vatNumber= oRS.Fields.Item("LICTRADNUM").Value.ToString();
                        oDocument.LoadTotals(this.CompanyConnection);
                        oDocument.DefineType();
                        this.ListDocuments.Add(oDocument);
                        oRS.MoveNext();
                        //iResult+=this.LoadDocuments()
                    }

                    iRetVal++;
                }
                catch (Exception ex)
                {
                    Logging.WriteToLog("_sSQL=" + sSQL, Logging.LogStatus.RET_VAL);
                    var a = new Logging("myDataMethods.LoadnCreateClass.LoadDocumentsProcess", ex);
                }
                return iRetVal;
            }

            private int PrepareDocumentsProcess()
            {
                int iRetVal = 0;
                try
                {
                    int iResult = 0;
                    int iSuccess = this.ListDocuments.Count;

                    for (int i = 0; i < this.ListDocuments.Count; i++)
                    {
                        BoDocument oTemp = new BoDocument();
                        oTemp = this.ListDocuments[i];
                        int iTempResult = this.PrepareDocument(ref oTemp);
                        iResult += iTempResult;
                        if (iTempResult == 1)
                        {
                            //iRetVal++;
                            oTemp.DocumentStatus = DocumentPrepared.p_Success;
                        }
                        else
                        {
                            Logging.WriteToLog("Error Found On Document:" + oTemp.ObjType + " / " + oTemp.DocNum + "", Logging.LogStatus.ERROR);
                            oTemp.DocumentStatus = DocumentPrepared.pFailure;
                            this.SetIgnoreDue2Error(oTemp);
                        }
                        this.ListDocuments[i] = oTemp;
                    }

                    if (iResult == iSuccess)
                    {
                        iRetVal++;
                    }
                }
                catch (Exception ex)
                {
                    var a = new Logging("myDataMethods.LoadnCreateClass.LoadDocuments", ex);
                }
                return iRetVal;
            }

            private int PrepareDocument(ref BoDocument _oDocument)
            {
                int iRetVal = 0;
                int iResult = 0;
                string sSQL = "";
                try
                {
                    if (_oDocument.DocumentType == Enumerators.DocumentType.p_EU_TX) //είναι expense και δεν έχει mark => είναι δικό μας document EU/TX
                    {
                        //if (this.CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                        //{
                        //    sSQL = "SELECT F_GET_COUNTRY_REGION('" + _oDocument.ObjType + "','" + _oDocument.DocEntry + "') as region from dummy";
                        //}
                        //else
                        //{
                        //    sSQL = "SELECT [dbo].[F_GET_COUNTRY_REGION] ('" + _oDocument.ObjType + "','" + _oDocument.DocEntry + "') as region";
                        //}
                        //
                        //string region = CommonLibrary.Functions.Database.ReturnDBValues(sSQL, "region", this.CompanyConnection).ToString();
                        //if (region.Equals("TX") || region.Equals("EU"))
                        //{
                        iResult = LoadFullDocumentData(ref _oDocument);
                        //}

                        if (iResult == 1)
                        {
                            iRetVal++;
                        }

                    }
                    else if (_oDocument.DocumentType == Enumerators.DocumentType.p_Income)
                    {
                        iResult = 0;
                        int iSuccess = 7;

                        iResult = LoadFullDocumentData(ref _oDocument);
                        if (iResult == 1)
                        {
                            iRetVal++;
                        }
                    }
                    else if (_oDocument.DocumentType == Enumerators.DocumentType.p_Reject) //reject
                    {

                        CommonLibrary.Ini.IniFile ini = new CommonLibrary.Ini.IniFile("C:\\Program Files\\SAP\\SAPmyDataService\\ConfParams.ini");
                        string LicTradNum = ini.IniReadValue("Default", "ISSUER_VAT_ID").ToString();
                        _oDocument.AADEMatchingDocument = new ExpensesClassificationsDoc();
                        _oDocument.AADEMatchingDocument.expensesInvoiceClassification = new List<InvoiceExpensesClassificationType>();
                        InvoiceExpensesClassificationType invoiceExpensesClassificationType = new InvoiceExpensesClassificationType();
                        invoiceExpensesClassificationType.invoiceMark = _oDocument.MARK;
                        invoiceExpensesClassificationType.entityVatNumber = LicTradNum;
                        invoiceExpensesClassificationType.Items = new object[] { 1 };

                        _oDocument.AADEMatchingDocument.expensesInvoiceClassification.Add(invoiceExpensesClassificationType);
                        _oDocument.DocumentStatus = DocumentPrepared.p_Success;
                        
                        iRetVal++;
                       
                    }
                    else if (_oDocument.DocumentType == Enumerators.DocumentType.p_Deviation) //deviation
                    {
                        CommonLibrary.Ini.IniFile ini = new CommonLibrary.Ini.IniFile("C:\\Program Files\\SAP\\SAPmyDataService\\ConfParams.ini");
                        string LicTradNum = ini.IniReadValue("Default", "ISSUER_VAT_ID").ToString();
                        _oDocument.AADEMatchingDocument = new ExpensesClassificationsDoc();
                        _oDocument.AADEMatchingDocument.expensesInvoiceClassification = new List<InvoiceExpensesClassificationType>();
                        InvoiceExpensesClassificationType invoiceExpensesClassificationType = new InvoiceExpensesClassificationType();
                        invoiceExpensesClassificationType.invoiceMark = _oDocument.MARK;
                        invoiceExpensesClassificationType.entityVatNumber = LicTradNum;
                        invoiceExpensesClassificationType.Items = new object[] { 2 };

                        _oDocument.AADEMatchingDocument.expensesInvoiceClassification.Add(invoiceExpensesClassificationType);
                        _oDocument.DocumentStatus = DocumentPrepared.p_Success;

                        iRetVal++;

                    }
                    else if (_oDocument.DocumentType == Enumerators.DocumentType.p_Matched) //match
                    {
                        _oDocument.AADEDocument = new InvoicesDoc();
                        _oDocument.AADEDocument.invoice = new List<AadeBookInvoiceType>();
                        AadeBookInvoiceType oInvoiceType = new AadeBookInvoiceType();
                        int iTempHeader = 0;
                        oInvoiceType.invoiceHeader = new InvoiceHeaderType();
                        oInvoiceType.invoiceHeader = this.GetInvoiceHeader(ref _oDocument, out iTempHeader);
                        int iRes = 0;
                        if (_oDocument.ObjType.Equals("18") || _oDocument.ObjType.Equals("19"))
                        {
                            iRes = this.MatchExpenses(ref _oDocument, ref oInvoiceType);

                        }
                        else if (_oDocument.ObjType.Equals("13") || _oDocument.ObjType.Equals("14"))
                        {
                             iRes = this.MatchIncome(ref _oDocument, ref oInvoiceType);
                        }

                        if (iRes == 1)
                        {
                            iRetVal++;

                        }

                    }
                }
                catch (Exception ex)
                {
                    var a = new Logging("myDataMethods.LoadnCreateClass.LoadDocuments", ex);
                }
                return iRetVal;
            }



            private int MatchExpenses(ref BoDocument _oDocument, ref AadeBookInvoiceType oInvoiceType)
            {
                int iRetVal = 0;
                string sSQL = "";
                try
                {
                    _oDocument.AADEMatchingDocument = new ExpensesClassificationsDoc();
                    InvoiceExpensesClassificationType invoiceExpensesClassificationType = new InvoiceExpensesClassificationType();
                    invoiceExpensesClassificationType.invoiceMark = _oDocument.MARK;
                    string sFileLocation = "C:\\Program Files\\SAP\\SAPmyDataService\\ConfParams.ini";
                    CommonLibrary.Ini.IniFile ini = new CommonLibrary.Ini.IniFile(sFileLocation);
                    string vat = ini.IniReadValue("Default", "ISSUER_VAT_ID");
                    invoiceExpensesClassificationType.entityVatNumber = vat;

                    if (_oDocument.reject_deviation.Equals("ERROR_306"))
                    {
                        if (this.CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                        {
                            sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_DETAILS_SUMMARISED WHERE 1=1 AND \"ObjType\" = '" + _oDocument.ObjType + "' AND \"DocEntry\" = '" + _oDocument.DocEntry + "'";
                        }
                        else
                        {
                            sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_DETAILS_SUMMARISED WHERE 1=1 AND ObjType = '" + _oDocument.ObjType + "' AND DocEntry = '" + _oDocument.DocEntry + "'";
                        }
                        this.SetIgnoreDue2Error(_oDocument);
                    }
                    else
                    {
                        if (this.CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                        {
                            sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_DETAILS_WRAPPER WHERE 1=1 AND \"ObjType\" = '" + _oDocument.ObjType + "' AND \"DocEntry\" = '" + _oDocument.DocEntry + "'";
                        }
                        else
                        {
                            sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_DETAILS_WRAPPER WHERE 1=1 AND ObjType = '" + _oDocument.ObjType + "' AND DocEntry = '" + _oDocument.DocEntry + "'";
                        }
                    }

                    SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, this.CompanyConnection);

                    int iRow = 1;
                    _oDocument.AADEMatchingDocument.expensesInvoiceClassification = new List<InvoiceExpensesClassificationType>();
                    List<object> ItemsList = new List<object>();
                    //invoiceExpensesClassificationType.entityVatNumber = _oDocument.CounterPart_vatNumber;
                    while (oRS.EoF == false)
                    {


                        InvoicesExpensesClassificationDetailType invoicesExpensesClassificationDetails = new InvoicesExpensesClassificationDetailType();
                        invoicesExpensesClassificationDetails.lineNumber = iRow;
                        invoicesExpensesClassificationDetails.expensesClassificationDetailData = new List<ExpensesClassificationType>();
                        ExpensesClassificationType expensesClassificationType = new ExpensesClassificationType();
                        expensesClassificationType.id = 1;
                        decimal amount = decimal.Parse(Math.Round((double.Parse(oRS.Fields.Item("ClassificationTypeAmount").Value.ToString())), 2).ToString("0.00"));
                        expensesClassificationType.amount = decimal.Round(amount, 2).ToString("0.00").Replace(",", ".");
                        //expensesClassificationType.amount = Math.Round((double.Parse(oRS.Fields.Item("ClassificationTypeAmount").Value.ToString())), 2);
                        expensesClassificationType.classificationCategory = (ExpensesClassificationCategoryType)Enum.Parse(typeof(ExpensesClassificationCategoryType), oRS.Fields.Item("classificationCategory").Value.ToString());
                        ExpensesClassificationCategoryType category = expensesClassificationType.classificationCategory;
                        string classificationType = oRS.Fields.Item("classificationType").Value.ToString();
                        if ((!string.IsNullOrEmpty(classificationType) && !classificationType.Equals("-112")) && expensesClassificationType.classificationCategory != ExpensesClassificationCategoryType.category2_95)
                        {
                            expensesClassificationType.classificationType = (ExpensesClassificationTypeClassificationType)Enum.Parse(typeof(ExpensesClassificationTypeClassificationType), classificationType);
                            expensesClassificationType.classificationTypeSpecified = true;
                        }
                        else
                        {
                            expensesClassificationType.classificationTypeSpecified = false;
                        }
                        expensesClassificationType.idSpecified = true;
                        expensesClassificationType.classificationCategorySpecified = true;

                        #region New Method
                        if (_oDocument.useNewMethod == 1)
                        {
                            expensesClassificationType.vatAmount = decimal.Parse(oRS.Fields.Item("vatAmount").Value.ToString()); //should read from view
                            expensesClassificationType.vatAmountSpecified = true;
                            expensesClassificationType.vatCategory = int.Parse(oRS.Fields.Item("vatCategory").Value.ToString()); //should read from view 
                            expensesClassificationType.vatCategorySpecified = true;
                            invoiceExpensesClassificationType.classificationPostModeSpecified = false;
                            invoiceExpensesClassificationType.classificationPostMode = 1;
                            if (string.IsNullOrEmpty(oRS.Fields.Item("vatExemptionCategory").Value.ToString()) || oRS.Fields.Item("vatExemptionCategory").Value.ToString().Equals("-112") || oRS.Fields.Item("vatExemptionCategory").Value.ToString().Equals("0"))
                            {
                                expensesClassificationType.vatExemptionCategorySpecified = false;
                            }
                            else
                            {
                                expensesClassificationType.vatExemptionCategory = int.Parse(oRS.Fields.Item("vatExemptionCategory").Value.ToString()); //should read from view
                                expensesClassificationType.vatExemptionCategorySpecified = true;
                            }
                        }
                        else
                        {
                            invoiceExpensesClassificationType.classificationPostModeSpecified = false;
                            expensesClassificationType.vatAmountSpecified = false;
                            expensesClassificationType.vatCategorySpecified = false;
                            expensesClassificationType.vatExemptionCategorySpecified = false;
                        }
                        #endregion
                        invoicesExpensesClassificationDetails.expensesClassificationDetailData.Add(expensesClassificationType);


                        #region Add Vat data
                        double VatAmount = Math.Round((double.Parse(oRS.Fields.Item("vatAmount").Value.ToString())), 2);
                        string sNoVATCategories = ini.IniReadValue("Default", "NO_VAT_CATEGORIES");
                        List<string> ListNoVATCat = new List<string>();
                        ListNoVATCat = sNoVATCategories.Split(',').ToList();

                        string sNoVATInvoiceType = ini.IniReadValue("Default", "NO_VAT_INVOICE_TYPE");
                        List<string> ListNoVATType = new List<string>();
                        ListNoVATType = sNoVATInvoiceType.Split(',').ToList();

                        if (/*category != ExpensesClassificationCategoryType.category2_95 &&*/ VatAmount > 0 && (ListNoVATCat.Contains(expensesClassificationType.classificationCategory.ToString()) == false && ListNoVATType.Contains(oInvoiceType.invoiceHeader.invoiceType.ToString()) == false))
                        {
                            // if (/*VatAmount > 0 &&*/ expensesClassificationType.classificationCategory!= ExpensesClassificationCategoryType.category2_5)
                            //{
                            expensesClassificationType = new ExpensesClassificationType();
                            expensesClassificationType.id = 2;
                            amount = decimal.Parse(Math.Round((double.Parse(oRS.Fields.Item("ClassificationTypeAmount").Value.ToString())), 2).ToString("0.00"));
                            expensesClassificationType.amount = decimal.Round(amount, 2).ToString("0.00").Replace(",", ".");
                            if (category != ExpensesClassificationCategoryType.category2_95)
                            {
                                expensesClassificationType.classificationCategory = (ExpensesClassificationCategoryType)Enum.Parse(typeof(ExpensesClassificationCategoryType), oRS.Fields.Item("classificationCategory").Value.ToString());
                                expensesClassificationType.classificationCategorySpecified = true;
                            }
                            else
                            {
                                expensesClassificationType.classificationCategorySpecified = false;
                            }
                            string VATclassificationType = oRS.Fields.Item("VATclassificationType").Value.ToString();
                            if (!string.IsNullOrEmpty(VATclassificationType) && !VATclassificationType.Equals("-112") /*&& category != ExpensesClassificationCategoryType.category2_95*/)
                            {
                                expensesClassificationType.classificationType = (ExpensesClassificationTypeClassificationType)Enum.Parse(typeof(ExpensesClassificationTypeClassificationType), VATclassificationType);
                                expensesClassificationType.classificationTypeSpecified = true;
                            }
                            else
                            {
                                expensesClassificationType.classificationTypeSpecified = false;
                            }
                            expensesClassificationType.idSpecified = true;



                            #region New Method
                            if (_oDocument.useNewMethod == 1)
                            {
                                expensesClassificationType.vatAmount = decimal.Parse(oRS.Fields.Item("vatAmount").Value.ToString()); //should read from view
                                expensesClassificationType.vatAmountSpecified = true;
                                expensesClassificationType.vatCategory = int.Parse(oRS.Fields.Item("vatCategory").Value.ToString()); //should read from view 
                                expensesClassificationType.vatCategorySpecified = true;
                                invoiceExpensesClassificationType.classificationPostModeSpecified = false;
                                invoiceExpensesClassificationType.classificationPostMode = 1;
                                if (string.IsNullOrEmpty(oRS.Fields.Item("vatExemptionCategory").Value.ToString()) || oRS.Fields.Item("vatExemptionCategory").Value.ToString().Equals("-112") || oRS.Fields.Item("vatExemptionCategory").Value.ToString().Equals("0"))
                                {
                                    expensesClassificationType.vatExemptionCategorySpecified = false;
                                }
                                else
                                {
                                    expensesClassificationType.vatExemptionCategory = int.Parse(oRS.Fields.Item("vatExemptionCategory").Value.ToString()); //should read from view
                                    expensesClassificationType.vatExemptionCategorySpecified = true;
                                }
                            }
                            else
                            {
                                invoiceExpensesClassificationType.classificationPostModeSpecified = false;
                                expensesClassificationType.vatAmountSpecified = false;
                                expensesClassificationType.vatCategorySpecified = false;
                                expensesClassificationType.vatExemptionCategorySpecified = false;
                            }
                            #endregion
                            invoicesExpensesClassificationDetails.expensesClassificationDetailData.Add(expensesClassificationType);
                        }
                        #endregion

                        ItemsList.Add(invoicesExpensesClassificationDetails);
                        oRS.MoveNext();
                        iRow++;
                    }
                    if (_oDocument.DocumentType == Enumerators.DocumentType.p_Reject)
                    {
                        ItemsList.Add(new object[] { 1 });
                    }
                    if (_oDocument.DocumentType == Enumerators.DocumentType.p_Deviation)
                    {
                        ItemsList.Add(new object[] { 2 });
                    }
                    invoiceExpensesClassificationType.Items = ItemsList.ToArray();
                    _oDocument.AADEMatchingDocument.expensesInvoiceClassification.Add(invoiceExpensesClassificationType);
                    _oDocument.DocumentStatus = DocumentPrepared.p_Success;
                    iRetVal++;
                }
                catch (Exception ex)
                {
                    var a = new Logging("myDataMethods.LoadnCreateClass.MatchExpenses", ex);
                }
                return iRetVal;
            }


            private int MatchIncome(ref BoDocument _oDocument, ref AadeBookInvoiceType oInvoiceType)
            {
                int iRetVal = 0;
                string sSQL = "";
                try
                {
                    _oDocument.AADEMatchingDocumentIncome = new IncomeClassificationsDoc();
                    InvoiceIncomeClassificationType invoiceIncomeClassificationType = new InvoiceIncomeClassificationType();
                    invoiceIncomeClassificationType.invoiceMark = long.Parse(_oDocument.MARK);
                    string sFileLocation = "C:\\Program Files\\SAP\\SAPmyDataService\\ConfParams.ini";
                    CommonLibrary.Ini.IniFile ini = new CommonLibrary.Ini.IniFile(sFileLocation);
                    string vat = ini.IniReadValue("Default", "ISSUER_VAT_ID");
                    invoiceIncomeClassificationType.entityVatNumber = vat;

                    if (_oDocument.reject_deviation.Equals("ERROR_306"))
                    {
                        if (this.CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                        {
                            sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_DETAILS_SUMMARISED WHERE 1=1 AND \"ObjType\" = '" + _oDocument.ObjType + "' AND \"DocEntry\" = '" + _oDocument.DocEntry + "'";
                        }
                        else
                        {
                            sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_DETAILS_SUMMARISED WHERE 1=1 AND ObjType = '" + _oDocument.ObjType + "' AND DocEntry = '" + _oDocument.DocEntry + "'";
                        }
                        this.SetIgnoreDue2Error(_oDocument);
                    }
                    else
                    {
                        if (this.CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                        {
                            sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_DETAILS_WRAPPER WHERE 1=1 AND \"ObjType\" = '" + _oDocument.ObjType + "' AND \"DocEntry\" = '" + _oDocument.DocEntry + "'";
                        }
                        else
                        {
                            sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_DETAILS_WRAPPER WHERE 1=1 AND ObjType = '" + _oDocument.ObjType + "' AND DocEntry = '" + _oDocument.DocEntry + "'";
                        }
                    }

                    SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, this.CompanyConnection);

                    int iRow = 1;
                    _oDocument.AADEMatchingDocumentIncome.incomeInvoiceClassification = new List<InvoiceIncomeClassificationType>();
                    List<object> ItemsList = new List<object>();
                    //invoiceExpensesClassificationType.entityVatNumber = _oDocument.CounterPart_vatNumber;
                    while (oRS.EoF == false)
                    {


                        InvoicesIncomeClassificationDetailType invoicesIncomeClassificationDetails = new InvoicesIncomeClassificationDetailType();
                        invoicesIncomeClassificationDetails.lineNumber = iRow;
                        invoicesIncomeClassificationDetails.incomeClassificationDetailData = new List<IncomeClassificationType>();
                        IncomeClassificationType incomeClassificationType = new IncomeClassificationType();
                        incomeClassificationType.id = 1;
                        decimal amount = decimal.Parse(Math.Round((double.Parse(oRS.Fields.Item("ClassificationTypeAmount").Value.ToString())), 2).ToString("0.00"));
                        incomeClassificationType.amount = decimal.Round(amount, 2).ToString("0.00").Replace(",", ".");
                        //expensesClassificationType.amount = Math.Round((double.Parse(oRS.Fields.Item("ClassificationTypeAmount").Value.ToString())), 2);
                        incomeClassificationType.classificationCategory = (IncomeClassificationCategoryType)Enum.Parse(typeof(IncomeClassificationCategoryType), oRS.Fields.Item("classificationCategory").Value.ToString());
                        IncomeClassificationCategoryType category = incomeClassificationType.classificationCategory;
                        string classificationType = oRS.Fields.Item("classificationType").Value.ToString();
                        if (!string.IsNullOrEmpty(classificationType) && !classificationType.Equals("-112") && incomeClassificationType.classificationCategory != IncomeClassificationCategoryType.category1_95)
                        {
                            incomeClassificationType.classificationType = (IncomeClassificationValueType)Enum.Parse(typeof(IncomeClassificationValueType), classificationType);
                            incomeClassificationType.classificationTypeSpecified = true;
                        }
                        else
                        {
                            incomeClassificationType.classificationTypeSpecified = false;
                        }
                        incomeClassificationType.idSpecified = true;
                        incomeClassificationType.classificationCategorySpecified = true;
                        invoicesIncomeClassificationDetails.incomeClassificationDetailData.Add(incomeClassificationType);

                        ItemsList.Add(invoicesIncomeClassificationDetails);
                        oRS.MoveNext();
                        iRow++;
                    }

                    invoiceIncomeClassificationType.Items = ItemsList.ToArray();
                    _oDocument.AADEMatchingDocumentIncome.incomeInvoiceClassification.Add(invoiceIncomeClassificationType);
                    _oDocument.DocumentStatus = DocumentPrepared.p_Success;
                    iRetVal++;
                }
                catch (Exception ex)
                {
                    var a = new Logging("myDataMethods.LoadnCreateClass.MatchIncome", ex);
                }
                return iRetVal;
            }



            private int LoadFullDocumentData(ref BoDocument _oDocument)
            {
                int iResult = 0;
                int iRetVal = 0;
                int iSuccess = 7;
                try
                {
                    int iTempHeader, iTempPayment, iTempIssuer, iTempCounterPart, iTempTaxesTotals, iTempDocumentSummary, iTempDetails;
                    iTempHeader = iTempPayment = iTempIssuer = iTempCounterPart = iTempTaxesTotals = iTempDocumentSummary = iTempDetails = 0;

                    _oDocument.AADEDocument = new InvoicesDoc();
                    _oDocument.AADEDocument.invoice = new List<AadeBookInvoiceType>();
                    AadeBookInvoiceType oInvoiceType = new AadeBookInvoiceType();

                    oInvoiceType.invoiceHeader = new InvoiceHeaderType();
                    oInvoiceType.invoiceHeader = this.GetInvoiceHeader(ref _oDocument, out iTempHeader);


                    CommonLibrary.Ini.IniFile ini = new CommonLibrary.Ini.IniFile("C:\\Program Files\\SAP\\SAPmyDataService\\ConfParams.ini");


                    string sPaymentMethods = ini.IniReadValue("Default", "PAYMENT_METHODS");
                    List<string> ListJEPaymentMethods = new List<string>();
                    ListJEPaymentMethods = sPaymentMethods.Split(',').ToList();


                    string sNoPaymentMethods = ini.IniReadValue("Default", "NO_PAYMENT_METHODS");
                    List<string> ListNoPaymentMethods = new List<string>();
                    ListNoPaymentMethods = sNoPaymentMethods.Split(',').ToList();


                    if ((_oDocument.ObjType != "30" && _oDocument.ObjType != "31") || ((_oDocument.ObjType == "30" || _oDocument.ObjType == "31") && ListJEPaymentMethods.Contains(oInvoiceType.invoiceHeader.invoiceType.ToString()) == true))
                    {
                        if (_oDocument.DocumentType == Enumerators.DocumentType.p_Income || (_oDocument.DocumentType == Enumerators.DocumentType.p_EU_TX && ListNoPaymentMethods.Contains(oInvoiceType.invoiceHeader.invoiceType.ToString()) == false))
                        {
                            oInvoiceType.paymentMethods = new List<PaymentMethodDetailType>();
                            oInvoiceType.paymentMethods = this.GetPaymentMethods(_oDocument, out iTempPayment);
                        }
                        else
                        {
                            iTempPayment++;
                        }


                        oInvoiceType.counterpart = new PartyType();
                        if (_oDocument.DocumentType == Enumerators.DocumentType.p_Income /*|| _oDocument.DocumentType == Enumerators.DocumentType.p_EU_TX*/)
                        {
                            oInvoiceType.counterpart = this.GetCounterPart(_oDocument, out iTempCounterPart, oInvoiceType.invoiceHeader.invoiceType);
                        }
                        else
                        {

                            oInvoiceType.counterpart = this.GetIssuer(out iTempIssuer, _oDocument);

                        }

                    }
                    else
                    {
                        iTempPayment++;
                        iTempCounterPart++;
                    }

                    oInvoiceType.issuer = new PartyType();
                    if (_oDocument.DocumentType == Enumerators.DocumentType.p_Income /*|| _oDocument.DocumentType == Enumerators.DocumentType.p_EU_TX*/)
                    {
                        oInvoiceType.issuer = this.GetIssuer(out iTempIssuer, _oDocument);
                    }
                    else
                    {
                        oInvoiceType.issuer = this.GetCounterPart(_oDocument, out iTempCounterPart, oInvoiceType.invoiceHeader.invoiceType);
                    }

                    if (_oDocument.TotalTaxesAmount > 0)
                    {
                        List<TaxTotalsType> ListRet = new List<TaxTotalsType>();
                        ListRet = this.GetTaxesTotals(ref _oDocument, out iTempTaxesTotals);
                        oInvoiceType.taxesTotals = ListRet.ToArray();
                    }
                    else
                    {
                        iTempTaxesTotals = 1;
                    }

                    oInvoiceType.invoiceDetails = new List<InvoiceRowType>();
                    oInvoiceType.invoiceDetails = this.GetDetails(_oDocument, oInvoiceType.invoiceHeader.invoiceType.ToString(), out iTempDetails);

                    oInvoiceType.invoiceSummary = new InvoiceSummaryType();
                    oInvoiceType.invoiceSummary = this.GetInvoiceSummary(_oDocument, out iTempDocumentSummary);

                    _oDocument.AADEDocument.invoice.Add(oInvoiceType);

                    iResult = iTempHeader + iTempPayment + iTempIssuer + iTempCounterPart + iTempTaxesTotals + iTempDocumentSummary + iTempDetails;

                    if (iResult == iSuccess)
                    {
                        iRetVal++;
                        _oDocument.DocumentStatus = DocumentPrepared.p_Success;
                    }
                    //else
                    //{
                    //    Logging.WriteToLog("Error Found On Document:" + _oDocument.ObjType + " / " + _oDocument.DocNum + "", Logging.LogStatus.ERROR);
                    //    _oDocument.DocumentStatus = DocumentPrepared.pFailure;
                    //    this.SetIgnoreDue2Error(_oDocument);
                    //}
                }
                catch (Exception ex)
                {
                    var a = new Logging("myDataMethods.LoadnCreateClass.LoadDocuments", ex);
                }
                return iRetVal;
            }

            /// <summary>
            /// Δημιουργία Γραμμών Παραστατικού
            /// </summary>
            /// <param name="_oDocument">Το Αντικείμενο του Παραστατικού</param>
            /// <param name="_iResult">1 For Success, 0 For Failure</param>
            /// <returns>Το Αντικείμενο της ΑΑΔΕ για τις γραμμές του Παραστατικού</returns>
            private List<InvoiceRowType> GetDetails(BoDocument _oDocument, string invoiceType, out int _iResult)
            {
                _iResult = 0;
                List<InvoiceRowType> oRet = new List<InvoiceRowType>();
                string sSQL = "";
                try
                {
                    InvoiceRowType oRow = null;
                    IncomeClassificationType oIncomeClassificationType = null;
                    ExpensesClassificationType oExpensesClassificationType = null;

                    if (this.CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_DETAILS_WRAPPER WHERE 1=1 AND \"ObjType\" = '" + _oDocument.ObjType + "' AND \"DocEntry\" = '" + _oDocument.DocEntry + "'";
                    }
                    else
                    {
                        sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_DETAILS_WRAPPER WHERE 1=1 AND ObjType = '" + _oDocument.ObjType + "' AND DocEntry = '" + _oDocument.DocEntry + "'";
                    }

                    SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, this.CompanyConnection);

                    int iRow = 0;
                    while (oRS.EoF == false)
                    {
                        iRow++;
                        oRow = new InvoiceRowType();

                        #region Required
                        oRow.lineNumber = iRow;
                        //oRow.netValue = double.Parse(oRS.Fields.Item("netValue").Value.ToString());
                        oRow.netValue = Math.Round((double.Parse(oRS.Fields.Item("netValue").Value.ToString())), 2);
                        //decimal amount = decimal.Parse(Math.Round((double.Parse(oRS.Fields.Item("netValue").Value.ToString())), 2).ToString("0.00"));
                        //oRow.netValue = decimal.Round(amount, 2).ToString("0.00");
                        oRow.vatCategory = int.Parse(oRS.Fields.Item("vatCategory").Value.ToString());
                        //oRow.vatAmount = double.Parse(oRS.Fields.Item("vatAmount").Value.ToString());
                        oRow.vatAmount = Math.Round((double.Parse(oRS.Fields.Item("vatAmount").Value.ToString())), 2);

                        _oDocument.isExpense = int.Parse(oRS.Fields.Item("IsExpense").Value.ToString());
                        if (_oDocument.isExpense == 0)
                        {
                            oIncomeClassificationType = new IncomeClassificationType();
                            decimal amount = decimal.Parse(Math.Round((double.Parse(oRS.Fields.Item("ClassificationTypeAmount").Value.ToString())), 2).ToString("0.00"));
                            oIncomeClassificationType.amount = decimal.Round(amount, 2).ToString("0.00").Replace(",", ".");

                            oIncomeClassificationType.classificationCategory = (IncomeClassificationCategoryType)Enum.Parse(typeof(IncomeClassificationCategoryType), oRS.Fields.Item("classificationCategory").Value.ToString());
                            string classificationType = oRS.Fields.Item("classificationType").Value.ToString();
                            if (!string.IsNullOrEmpty(classificationType) && !classificationType.Equals("-112") && oIncomeClassificationType.classificationCategory != IncomeClassificationCategoryType.category1_95)
                            {
                                oIncomeClassificationType.classificationType = (IncomeClassificationValueType)Enum.Parse(typeof(IncomeClassificationValueType), classificationType);
                                oIncomeClassificationType.classificationTypeSpecified = true;

                            }
                            else
                            {
                                oIncomeClassificationType.classificationTypeSpecified = false;
                            }
                            oIncomeClassificationType.idSpecified = false;
                            oIncomeClassificationType.classificationCategorySpecified = true;

                            oRow.incomeClassification = new List<IncomeClassificationType>();
                            oRow.incomeClassification.Add(oIncomeClassificationType);
                        }
                        else
                        {
                            oRow.expensesClassification = new List<ExpensesClassificationType>();
                            oExpensesClassificationType = new ExpensesClassificationType();
                            //oExpensesClassificationType.amount = Math.Round((double.Parse(oRS.Fields.Item("ClassificationTypeAmount").Value.ToString())), 2);
                            decimal amount = decimal.Parse(Math.Round((double.Parse(oRS.Fields.Item("ClassificationTypeAmount").Value.ToString())), 2).ToString("0.00"));
                            oExpensesClassificationType.amount = decimal.Round(amount, 2).ToString("0.00").Replace(",", ".");
                            oExpensesClassificationType.classificationCategory = /*ExpensesClassificationCategoryType.category2_1;*/ (ExpensesClassificationCategoryType)Enum.Parse(typeof(ExpensesClassificationCategoryType), oRS.Fields.Item("classificationCategory").Value.ToString());
                            string classificationType = oRS.Fields.Item("classificationType").Value.ToString();
                            if (!string.IsNullOrEmpty(classificationType) && !classificationType.Equals("-112") && oExpensesClassificationType.classificationCategory != ExpensesClassificationCategoryType.category2_95)
                            {
                                oExpensesClassificationType.classificationType = /*ExpensesClassificationTypeClassificationType.E3_101;*/(ExpensesClassificationTypeClassificationType)Enum.Parse(typeof(ExpensesClassificationTypeClassificationType), classificationType);
                                oExpensesClassificationType.classificationTypeSpecified = true;
                            }
                            else
                            {
                                oExpensesClassificationType.classificationTypeSpecified = false;
                            }
                            oExpensesClassificationType.classificationCategorySpecified = true;
                            oExpensesClassificationType.idSpecified = true;
                            oExpensesClassificationType.id = 1;

                            oRow.expensesClassification.Add(oExpensesClassificationType);

                            string sFileLocation = "C:\\Program Files\\SAP\\SAPmyDataService\\ConfParams.ini";
                            CommonLibrary.Ini.IniFile ini = new CommonLibrary.Ini.IniFile(sFileLocation);

                            string sNoVATCategories = ini.IniReadValue("Default", "NO_VAT_CATEGORIES");
                            List<string> ListNoVATCat = new List<string>();
                            ListNoVATCat = sNoVATCategories.Split(',').ToList();

                            string sNoVATInvoiceType = ini.IniReadValue("Default", "NO_VAT_INVOICE_TYPE");
                            List<string> ListNoVATType = new List<string>();
                            ListNoVATType = sNoVATInvoiceType.Split(',').ToList();

                            if ((!_oDocument.ObjType.Equals("30") && !_oDocument.ObjType.Equals("31")) /*&& !_oDocument.ObjType.Equals("444")*/ && ListNoVATCat.Contains(oExpensesClassificationType.classificationCategory.ToString()) == false && ListNoVATType.Contains(invoiceType) == false)
                            {

                                oExpensesClassificationType = new ExpensesClassificationType();
                                //oExpensesClassificationType.amount = Math.Round((double.Parse(oRS.Fields.Item("vatAmount").Value.ToString())), 2);
                                amount = decimal.Parse(Math.Round((double.Parse(oRS.Fields.Item("ClassificationTypeAmount").Value.ToString())), 2).ToString("0.00"));
                                oExpensesClassificationType.amount = decimal.Round(amount, 2).ToString("0.00").Replace(",", ".");
                                oExpensesClassificationType.classificationCategory = /*ExpensesClassificationCategoryType.category2_1;*/ (ExpensesClassificationCategoryType)Enum.Parse(typeof(ExpensesClassificationCategoryType), oRS.Fields.Item("classificationCategory").Value.ToString());
                                string VATclassificationType = oRS.Fields.Item("VATclassificationType").Value.ToString();
                                if (!string.IsNullOrEmpty(VATclassificationType) && !VATclassificationType.Equals("-112"))
                                {
                                    oExpensesClassificationType.classificationType = /*ExpensesClassificationTypeClassificationType.VAT_364;*/(ExpensesClassificationTypeClassificationType)Enum.Parse(typeof(ExpensesClassificationTypeClassificationType), VATclassificationType);
                                    oExpensesClassificationType.classificationTypeSpecified = true;
                                }
                                else
                                {
                                    oExpensesClassificationType.classificationTypeSpecified = false;
                                }
                                oExpensesClassificationType.classificationCategorySpecified = true;
                                oExpensesClassificationType.idSpecified = true;
                                oExpensesClassificationType.id = 2;

                                oRow.expensesClassification.Add(oExpensesClassificationType);

                            }


                        }
                        #endregion

                        #region Not Required
                        //TODO
                        //if (string.IsNullOrEmpty(dtRow["aaaa"].ToString()))
                        //{
                        //    oRow.dienergia = ;
                        //}

                        if (string.IsNullOrEmpty(oRS.Fields.Item("lineComments").Value.ToString()))
                        {
                            oRow.lineComments = oRS.Fields.Item("lineComments").Value.ToString();
                        }

                        if (string.IsNullOrEmpty(oRS.Fields.Item("quantity").Value.ToString()) || double.Parse(oRS.Fields.Item("quantity").Value.ToString()) == -1)
                        {
                            oRow.quantitySpecified = false;
                        }
                        else
                        {
                            oRow.quantitySpecified = true;
                            //oRow.quantity = double.Parse(oRS.Fields.Item("quantity"].ToString());
                            oRow.quantity = Math.Round((double.Parse(oRS.Fields.Item("quantity").Value.ToString())), 2);
                        }

                        if (string.IsNullOrEmpty(oRS.Fields.Item("deductionsAmount").Value.ToString()) || int.Parse(oRS.Fields.Item("deductionsAmount").Value.ToString()) == -112)
                        {
                            oRow.deductionsAmountSpecified = false;
                        }
                        else
                        {
                            oRow.deductionsAmountSpecified = true;
                            //oRow.deductionsAmount = double.Parse(oRS.Fields.Item("deductionsAmount").Value.ToString());
                            oRow.deductionsAmount = Math.Round((double.Parse(oRS.Fields.Item("deductionsAmount").Value.ToString())), 2);
                        }

                        if (string.IsNullOrEmpty(oRS.Fields.Item("otherTaxesPercentCategory").Value.ToString()) || int.Parse(oRS.Fields.Item("otherTaxesPercentCategory").Value.ToString()) == -112)
                        {
                            oRow.otherTaxesPercentCategorySpecified = false;
                        }
                        else
                        {
                            oRow.otherTaxesPercentCategorySpecified = true;
                            oRow.otherTaxesPercentCategory = int.Parse(oRS.Fields.Item("otherTaxesPercentCategory").Value.ToString());

                            if (string.IsNullOrEmpty(oRS.Fields.Item("otherTaxesAmount").Value.ToString()))
                            {
                                oRow.otherTaxesAmountSpecified = false;
                            }
                            else
                            {
                                oRow.otherTaxesAmountSpecified = true;
                                //oRow.otherTaxesAmount = double.Parse(oRS.Fields.Item("otherTaxesAmount").Value.ToString());
                                oRow.otherTaxesAmount = Math.Round((double.Parse(oRS.Fields.Item("otherTaxesAmount").Value.ToString())), 2);
                            }
                        }

                        if (string.IsNullOrEmpty(oRS.Fields.Item("feesPercentCategory").Value.ToString()) || int.Parse(oRS.Fields.Item("feesPercentCategory").Value.ToString()) == -112)
                        {
                            oRow.feesPercentCategorySpecified = false;
                        }
                        else
                        {
                            oRow.feesPercentCategorySpecified = true;
                            oRow.feesPercentCategory = int.Parse(oRS.Fields.Item("feesPercentCategory").Value.ToString());

                            if (string.IsNullOrEmpty(oRS.Fields.Item("feesAmount").Value.ToString()))
                            {
                                oRow.feesAmountSpecified = false;
                            }
                            else
                            {
                                oRow.feesAmountSpecified = true;
                                //oRow.feesAmount = double.Parse(oRS.Fields.Item("feesAmount"].ToString());
                                oRow.feesAmount = Math.Round((double.Parse(oRS.Fields.Item("feesAmount").Value.ToString())), 2);
                            }
                        }

                        if (string.IsNullOrEmpty(oRS.Fields.Item("stampDutyPercentCategory").Value.ToString()) || int.Parse(oRS.Fields.Item("stampDutyPercentCategory").Value.ToString()) == -112)
                        {
                            oRow.stampDutyPercentCategorySpecified = false;
                        }
                        else
                        {
                            oRow.stampDutyPercentCategorySpecified = true;
                            oRow.stampDutyPercentCategory = int.Parse(oRS.Fields.Item("stampDutyPercentCategory").Value.ToString());

                            if (string.IsNullOrEmpty(oRS.Fields.Item("stampDutyAmount").Value.ToString()))
                            {
                                oRow.stampDutyAmountSpecified = false;
                            }
                            else
                            {
                                oRow.stampDutyAmountSpecified = true;
                                //oRow.stampDutyAmount = double.Parse(oRS.Fields.Item("stampDutyAmount").Value.ToString());
                                oRow.stampDutyAmount = Math.Round((double.Parse(oRS.Fields.Item("stampDutyAmount").Value.ToString())), 2);
                            }
                        }

                        if (string.IsNullOrEmpty(oRS.Fields.Item("withheldPercentCategory").Value.ToString()) || double.Parse(oRS.Fields.Item("withheldPercentCategory").Value.ToString()) == -112)
                        {
                            oRow.withheldPercentCategorySpecified = false;
                        }
                        else
                        {
                            oRow.withheldPercentCategorySpecified = true;
                            oRow.withheldPercentCategory = int.Parse(oRS.Fields.Item("withheldPercentCategory").Value.ToString());

                            if (string.IsNullOrEmpty(oRS.Fields.Item("withheldAmount").Value.ToString()))
                            {
                                oRow.withheldAmountSpecified = false;
                            }
                            else
                            {
                                oRow.withheldAmountSpecified = true;
                                //oRow.withheldAmount = double.Parse(oRS.Fields.Item("withheldAmount").Value.ToString());
                                oRow.withheldAmount = Math.Round((double.Parse(oRS.Fields.Item("withheldAmount").Value.ToString())), 2);
                            }
                        }

                        if (string.IsNullOrEmpty(oRS.Fields.Item("discountOption").Value.ToString()))
                        {
                            oRow.discountOptionSpecified = false;
                        }
                        else
                        {
                            oRow.discountOptionSpecified = true;
                            oRow.discountOption = oRS.Fields.Item("discountOption").Value.ToString() == "false" ? false : true;
                        }

                        if (string.IsNullOrEmpty(oRS.Fields.Item("vatExemptionCategory").Value.ToString()) || int.Parse(oRS.Fields.Item("vatExemptionCategory").Value.ToString()) == -112 || oRS.Fields.Item("vatExemptionCategory").Value.ToString().Equals("0"))
                        {
                            oRow.vatExemptionCategorySpecified = false;
                        }
                        else
                        {
                            oRow.vatExemptionCategorySpecified = true;
                            oRow.vatExemptionCategory = int.Parse(oRS.Fields.Item("vatExemptionCategory").Value.ToString());
                        }

                        if (string.IsNullOrEmpty(oRS.Fields.Item("measurementUnit").Value.ToString()) || int.Parse(oRS.Fields.Item("measurementUnit").Value.ToString()) == -112)
                        {
                            oRow.measurementUnitSpecified = false;
                        }
                        else
                        {
                            oRow.measurementUnitSpecified = true;
                            oRow.measurementUnit = int.Parse(oRS.Fields.Item("measurementUnit").Value.ToString());
                        }

                        if (string.IsNullOrEmpty(oRS.Fields.Item("invoiceDetailType").Value.ToString()) || int.Parse(oRS.Fields.Item("invoiceDetailType").Value.ToString()) == -112)
                        {
                            oRow.invoiceDetailTypeSpecified = false;
                        }
                        else
                        {
                            oRow.invoiceDetailTypeSpecified = true;
                            oRow.invoiceDetailType = int.Parse(oRS.Fields.Item("invoiceDetailType").Value.ToString());
                        }

                        if (string.IsNullOrEmpty(oRS.Fields.Item("recType").Value.ToString()) || int.Parse(oRS.Fields.Item("recType").Value.ToString()) == -112)
                        {
                            oRow.recTypeSpecified = false;
                        }
                        else
                        {
                            oRow.recTypeSpecified = true;
                            oRow.recType = int.Parse(oRS.Fields.Item("recType").Value.ToString());
                        }

                        #endregion
                        //oRow.expensesClassification = null;

                        oRet.Add(oRow);

                        oRS.MoveNext();
                    }


                    _iResult++;
                }
                catch (Exception ex)
                {
                    Logging.WriteToLog("sSQL=" + sSQL, Logging.LogStatus.RET_VAL);
                    var a = new Logging("myDataMethods.LoadnCreateClass.GetDetails", ex);
                }
                return oRet;
            }

            private List<TaxTotalsType> GetTaxesTotals(ref BoDocument _oDocument, out int _iRetVal)
            {
                _iRetVal = 0;
                List<TaxTotalsType> oRet = new List<TaxTotalsType>();
                string sSQL = "";
                try
                {
                    TaxTotalsType oType = null;
                    oRet = new List<TaxTotalsType>();

                    if (this.CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_TAXES_TOTALS_WRAPPER WHERE 1=1 AND \"ObjType\" = '" + _oDocument.ObjType + "' AND \"DocEntry\" = '" + _oDocument.DocEntry + "'";
                    }
                    else
                    {
                        sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_TAXES_TOTALS_WRAPPER WHERE 1=1 AND ObjType = '" + _oDocument.ObjType + "' AND DocEntry = '" + _oDocument.DocEntry + "'";
                    }

                    SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, this.CompanyConnection);

                    while (oRS.EoF == false)
                    {
                        oType = new TaxTotalsType();
                        //oType.taxAmount = double.Parse(oRS.Fields.Item(["TAX_AMOUNT").Value.ToString());
                        oType.taxAmount = Math.Round((double.Parse(oRS.Fields.Item("TAX_AMOUNT").Value.ToString())), 2);
                        oType.taxCategorySpecified = true;
                        oType.taxCategory = int.Parse(oRS.Fields.Item("TAX_CATEGORY").Value.ToString());
                        oType.taxType = int.Parse(oRS.Fields.Item("TAX_CODE").Value.ToString());
                        oType.underlyingValueSpecified = true;
                        //oType.underlyingValue = double.Parse(oRS.Fields.Item("TAX_BASE_AMOUNT").Value.ToString());
                        oType.underlyingValue = Math.Round((double.Parse(oRS.Fields.Item("TAX_BASE_AMOUNT").Value.ToString())), 2);

                        oRet.Add(oType);

                        oRS.MoveNext();
                    }
                    _iRetVal++;
                }
                catch (Exception ex)
                {
                    var a = new Logging("myDataMethods.LoadnCreateClass.GetInvoiceSummary", ex);
                }
                return oRet;
            }

            /// <summary>
            /// Δημιουργία Totals Classifications
            /// </summary>
            /// <param name="_oIncomeClassification">Λίστα classifications εσόδων</param>
            /// <param name="_oExpensesClassification">Λίστα classifications εξόδων</param>
            /// <returns>1 for success, 0 for failure</returns>
            private int GetInvoiceTotalsClassifications(BoDocument _oDocument, out double dTotal, out List<IncomeClassificationType> _oIncomeClassification, out List<ExpensesClassificationType> _oExpensesClassification)
            {
                int iRetVal = 0;
                string sSQL = "";
                _oIncomeClassification = null;
                _oExpensesClassification = null;
                dTotal = 0;
                try
                {
                    if (_oDocument.isExpense == 0)
                    {
                        IncomeClassificationType oIncomeClassificationType = null;
                        _oIncomeClassification = new List<IncomeClassificationType>();



                        if (this.CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                        {
                            sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_TOTALS_WRAPPER WHERE 1=1 AND \"ObjType\" = '" + _oDocument.ObjType + "' AND \"DocEntry\" = '" + _oDocument.DocEntry + "'";
                        }
                        else
                        {
                            sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_TOTALS_WRAPPER WHERE 1=1 AND ObjType = '" + _oDocument.ObjType + "' AND DocEntry = '" + _oDocument.DocEntry + "'";
                        }

                        SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, this.CompanyConnection);

                        while (oRS.EoF == false)
                        {
                            dTotal += double.Parse(oRS.Fields.Item("Amount").Value.ToString());
                            oIncomeClassificationType = new IncomeClassificationType();
                            //oIncomeClassificationType.amount = double.Parse(oRS.Fields.Item("Amount").Value.ToString());
                            decimal amount = decimal.Parse(Math.Round((double.Parse(oRS.Fields.Item("Amount").Value.ToString())), 2).ToString("0.00"));
                            oIncomeClassificationType.amount = decimal.Round(amount, 2).ToString("0.00").Replace(",", ".");
                            oIncomeClassificationType.classificationCategory = (IncomeClassificationCategoryType)Enum.Parse(typeof(IncomeClassificationCategoryType), oRS.Fields.Item("classificationCategory").Value.ToString());
                            string classificationType = oRS.Fields.Item("classificationType").Value.ToString();
                            if (!string.IsNullOrEmpty(classificationType) && !classificationType.Equals("-112"))
                            {
                                oIncomeClassificationType.classificationType = (IncomeClassificationValueType)Enum.Parse(typeof(IncomeClassificationValueType), classificationType);
                                oIncomeClassificationType.classificationTypeSpecified = true;

                            }
                            else
                            {
                                oIncomeClassificationType.classificationTypeSpecified = false;
                            }
                            oIncomeClassificationType.idSpecified = false;
                            oIncomeClassificationType.classificationCategorySpecified = true;

                            _oIncomeClassification.Add(oIncomeClassificationType);

                            oRS.MoveNext();
                        }
                        iRetVal++;
                    }
                    else
                    {
                        ExpensesClassificationType oExpensesClassificationType = null;
                        _oExpensesClassification = new List<ExpensesClassificationType>();

                        dTotal = 0;

                        if (this.CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                        {
                            sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_TOTALS_EXPENSES_WRAPPER WHERE 1=1 AND \"ObjType\" = '" + _oDocument.ObjType + "' AND \"DocEntry\" = '" + _oDocument.DocEntry + "'";

                        }
                        else
                        {
                            sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_TOTALS_EXPENSES_WRAPPER WHERE 1=1 AND ObjType = '" + _oDocument.ObjType + "' AND DocEntry = '" + _oDocument.DocEntry + "'";

                        }

                        SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, this.CompanyConnection);
                        int id = 1;
                        while (oRS.EoF == false)
                        {
                            if (!oRS.Fields.Item("classificationType").Value.ToString().Contains("VAT"))
                            {
                                dTotal += double.Parse(oRS.Fields.Item("Amount").Value.ToString());
                            }
                            oExpensesClassificationType = new ExpensesClassificationType();
                            //oExpensesClassificationType.amount = Math.Round((double.Parse(oRS.Fields.Item("Amount").Value.ToString())), 2);
                            decimal amount = decimal.Parse(Math.Round((double.Parse(oRS.Fields.Item("Amount").Value.ToString())), 2).ToString("0.00"));
                            oExpensesClassificationType.amount = decimal.Round(amount, 2).ToString("0.00").Replace(",", ".");
                            oExpensesClassificationType.classificationCategory = /*ExpensesClassificationCategoryType.category2_1;*/ (ExpensesClassificationCategoryType)Enum.Parse(typeof(ExpensesClassificationCategoryType), oRS.Fields.Item("classificationCategory").Value.ToString());
                            string classificationType = oRS.Fields.Item("classificationType").Value.ToString();
                            if (!string.IsNullOrEmpty(classificationType) && !classificationType.Equals("-112"))
                            {
                                oExpensesClassificationType.classificationType = /*ExpensesClassificationTypeClassificationType.E3_101;*/(ExpensesClassificationTypeClassificationType)Enum.Parse(typeof(ExpensesClassificationTypeClassificationType), classificationType);
                                oExpensesClassificationType.classificationTypeSpecified = true;
                            }
                            else
                            {
                                oExpensesClassificationType.classificationTypeSpecified = false;
                            }
                            oExpensesClassificationType.classificationCategorySpecified = true;
                            oExpensesClassificationType.idSpecified = true;
                            oExpensesClassificationType.id = id;
                            _oExpensesClassification.Add(oExpensesClassificationType);
                            id++;

                            oRS.MoveNext();
                        }
                        iRetVal++;
                    }
                }
                catch (Exception ex)
                {
                    var a = new Logging("myDataMethods.LoadnCreateClass.GetInvoiceSummary", ex);
                }
                return iRetVal;
            }

            private InvoiceSummaryType GetInvoiceSummary(BoDocument _oDocument, out int _iResult)
            {
                _iResult = 0;
                InvoiceSummaryType oRet = new InvoiceSummaryType();
                string sSQL = "";
                try
                {
                    oRet = new InvoiceSummaryType();
                    List<IncomeClassificationType> oIncomeClassification = null;
                    List<ExpensesClassificationType> oExpensesClassification = null;
                    double _dTotal = 0;
                    int iResult = this.GetInvoiceTotalsClassifications(_oDocument, out _dTotal, out oIncomeClassification, out oExpensesClassification);
                    if (_oDocument.isExpense == 0)
                    {
                        oRet.incomeClassification = oIncomeClassification;
                    }
                    else
                    {
                        oRet.expensesClassification = oExpensesClassification;
                    }
                    double dTotal = _dTotal; ;
                    #region commented
                    //IncomeClassificationType oIncomeClassificationType = null;
                    //ExpensesClassificationType oExpensesClassificationType = null;

                    //if (this.CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    //{
                    //    //changed query from "TKA_V_ELECTRONIC_INVOICES_TOTALS" to "TKA_V_ELECTRONIC_INVOICES_TOTALS_WRAPPER" (3/6/2022)
                    //    sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_TOTALS_WRAPPER WHERE 1=1 AND \"ObjType\" = '" + _oDocument.ObjType + "' AND \"DocEntry\" = '" + _oDocument.DocEntry + "'";
                    //}
                    //else
                    //{
                    //    //changed query from "TKA_V_ELECTRONIC_INVOICES_TOTALS" to "TKA_V_ELECTRONIC_INVOICES_TOTALS_WRAPPER" (3/6/2022)
                    //    sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_TOTALS_WRAPPER WHERE 1=1 AND ObjType = '" + _oDocument.ObjType + "' AND DocEntry = '" + _oDocument.DocEntry + "'";
                    //}

                    //SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, this.CompanyConnection);
                    //string sIsExpense = oRS.Fields.Item("IsExpense").Value;

                    //while (oRS.EoF == false)
                    //{
                    //    dTotal += double.Parse(oRS.Fields.Item("Amount").Value.ToString());
                    //    if (Equals(sIsExpense, "0"))
                    //    {
                    //        oIncomeClassificationType = new IncomeClassificationType();
                    //        //oIncomeClassificationType.amount = double.Parse(oRS.Fields.Item("Amount").Value.ToString());
                    //        oIncomeClassificationType.amount = Math.Round((double.Parse(oRS.Fields.Item("Amount").Value.ToString())), 2);
                    //        oIncomeClassificationType.classificationCategory = (IncomeClassificationCategoryType)Enum.Parse(typeof(IncomeClassificationCategoryType), oRS.Fields.Item("classificationCategory").Value.ToString());
                    //        oIncomeClassificationType.classificationType = (IncomeClassificationValueType)Enum.Parse(typeof(IncomeClassificationValueType), oRS.Fields.Item("classificationType").Value.ToString());
                    //        oIncomeClassificationType.idSpecified = false;
                    //        oRet.incomeClassification.Add(oIncomeClassificationType);
                    //    }
                    //    else
                    //    {
                    //        oExpensesClassificationType = new ExpensesClassificationType();
                    //        //oIncomeClassificationType.amount = double.Parse(oRS.Fields.Item("Amount").Value.ToString());
                    //        oExpensesClassificationType.amount = Math.Round((double.Parse(oRS.Fields.Item("Amount").Value.ToString())), 2);
                    //        oExpensesClassificationType.classificationCategory = /*ExpensesClassificationCategoryType.category2_1;*/ (ExpensesClassificationCategoryType)Enum.Parse(typeof(ExpensesClassificationCategoryType), oRS.Fields.Item("classificationCategory").Value.ToString());
                    //        oExpensesClassificationType.classificationType = /*ExpensesClassificationTypeClassificationType.E3_101;*/(ExpensesClassificationTypeClassificationType)Enum.Parse(typeof(ExpensesClassificationTypeClassificationType), oRS.Fields.Item("classificationType").Value.ToString());
                    //        //oExpensesClassificationType.idSpecified = false;
                    //        oExpensesClassificationType.classificationTypeSpecified = true;
                    //        oExpensesClassificationType.classificationCategorySpecified = true;
                    //        oExpensesClassificationType.idSpecified = true;
                    //        oExpensesClassificationType.id = 1;
                    //        oRet.expensesClassification.Add(oExpensesClassificationType);

                    //        //used to test VAT in expenses
                    //        oExpensesClassificationType = new ExpensesClassificationType();
                    //        oExpensesClassificationType.amount = 0;
                    //        oExpensesClassificationType.classificationCategory = /*ExpensesClassificationCategoryType.category2_1;*/ (ExpensesClassificationCategoryType)Enum.Parse(typeof(ExpensesClassificationCategoryType), oRS.Fields.Item("classificationCategory").Value.ToString());
                    //        oExpensesClassificationType.classificationType = ExpensesClassificationTypeClassificationType.VAT_364;/*(ExpensesClassificationTypeClassificationType)Enum.Parse(typeof(ExpensesClassificationTypeClassificationType), oRS.Fields.Item("classificationType").Value.ToString());*/
                    //        oExpensesClassificationType.idSpecified = false;
                    //        oExpensesClassificationType.classificationTypeSpecified = true;
                    //        oExpensesClassificationType.classificationCategorySpecified = true;
                    //        oExpensesClassificationType.idSpecified = true;
                    //        oExpensesClassificationType.id = 2;
                    //        oRet.expensesClassification.Add(oExpensesClassificationType);

                    //    }
                    //    oRS.MoveNext();
                    //}
                    #endregion

                    //////////////////////////////////////////////////////////////////

                    if (this.CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        sSQL = "SELECT SUM(TAX_AMOUNT) AS \"Result\"," + Environment.NewLine +
                            " TAX_CODE" + Environment.NewLine +
                            " FROM TKA_V_ELECTRONIC_INVOICES_TAXES_TOTALS_WRAPPER" + Environment.NewLine +
                            " WHERE 1 = 1" + Environment.NewLine +
                            " AND \"ObjType\" = '" + _oDocument.ObjType + "'" + Environment.NewLine +
                            " AND \"DocEntry\" = '" + _oDocument.DocEntry + "'" + Environment.NewLine +
                            " GROUP BY TAX_CODE";
                    }
                    else
                    {
                        sSQL = "SELECT SUM(TAX_AMOUNT) AS Result," + Environment.NewLine +
                            " TAX_CODE" + Environment.NewLine +
                            " FROM TKA_V_ELECTRONIC_INVOICES_TAXES_TOTALS_WRAPPER" + Environment.NewLine +
                            " WHERE 1 = 1" + Environment.NewLine +
                            " AND ObjType = '" + _oDocument.ObjType + "'" + Environment.NewLine +
                            " AND DocEntry = '" + _oDocument.DocEntry + "'" + Environment.NewLine +
                            " GROUP BY TAX_CODE";
                    }
                    SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, this.CompanyConnection);
                    oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, this.CompanyConnection);
                    double dTotalFees, dTotalStamp, dTotalDeductions, dTotalOtherTaxes, dTotalWithheldTaxes;
                    dTotalFees = dTotalStamp = dTotalDeductions = dTotalOtherTaxes = dTotalWithheldTaxes = 0.00;

                    while (oRS.EoF == false)
                    {
                        switch ((string)oRS.Fields.Item("TAX_CODE").Value.ToString())
                        {
                            case "1":
                                dTotalWithheldTaxes = double.Parse(CommonLibrary.Functions.Database.ReturnDBValues(sSQL, "Result", this.CompanyConnection).ToString());
                                break;
                            case "2":
                                dTotalFees = double.Parse(CommonLibrary.Functions.Database.ReturnDBValues(sSQL, "Result", this.CompanyConnection).ToString());
                                break;
                            case "3":
                                dTotalOtherTaxes = double.Parse(CommonLibrary.Functions.Database.ReturnDBValues(sSQL, "Result", this.CompanyConnection).ToString());
                                break;
                            case "4":
                                dTotalStamp = double.Parse(CommonLibrary.Functions.Database.ReturnDBValues(sSQL, "Result", this.CompanyConnection).ToString());
                                break;
                            case "5":
                                dTotalDeductions = double.Parse(CommonLibrary.Functions.Database.ReturnDBValues(sSQL, "Result", this.CompanyConnection).ToString());
                                break;
                        }
                        oRS.MoveNext();
                    }

                    //***NOTE*** ALL FIELDS ARE REQUIRED!!!!
                    if (_oDocument.isExpense == 0)
                    {
                        #region commented
                        //oRet.totalDeductionsAmount = 0.00;
                        //oRet.totalFeesAmount = 0.00;
                        //oRet.totalGrossValue = dTotal + _oDocument.TotalVATAmount;//Net + taxes (Το Taxes περιλαμβάνει όλους τους επιπλέον φόρους βλ. View Φόρων)
                        //oRet.totalGrossValue = Math.Round((dTotal + _oDocument.TotalVATAmount), 2);//Net + taxes (Το Taxes περιλαμβάνει όλους τους επιπλέον φόρους βλ. View Φόρων)
                        //oRet.totalNetValue = dTotal;
                        //oRet.totalOtherTaxesAmount = 0.00;
                        //oRet.totalStampDutyAmount = 0.00;
                        //oRet.totalWithheldAmount = 0.00;
                        #endregion
                        oRet.totalDeductionsAmount = Math.Round(dTotalDeductions, 2);
                        oRet.totalFeesAmount = Math.Round(dTotalFees, 2);
                        oRet.totalNetValue = Math.Round(dTotal, 2);
                        oRet.totalOtherTaxesAmount = Math.Round(dTotalOtherTaxes, 2);
                        oRet.totalStampDutyAmount = Math.Round(dTotalStamp, 2);
                        oRet.totalVatAmount = Math.Round((_oDocument.TotalVATAmount), 2);
                        oRet.totalWithheldAmount = Math.Round(dTotalWithheldTaxes, 2);
                        oRet.expensesClassification = null;
                        oRet.totalGrossValue = Math.Round((dTotal + _oDocument.TotalVATAmount - dTotalDeductions - dTotalFees + dTotalStamp - dTotalOtherTaxes - dTotalWithheldTaxes), 2);//Net + taxes (Το Taxes περιλαμβάνει όλους τους επιπλέον φόρους βλ. View Φόρων)
                    }
                    else
                    {
                        #region commented
                        //TODO: check if this needs to be changed 
                        //oRet.totalDeductionsAmount = 0.00;
                        //oRet.totalFeesAmount = 0.00;
                        //oRet.totalGrossValue = dTotal + _oDocument.TotalVATAmount;//Net + taxes (Το Taxes περιλαμβάνει όλους τους επιπλέον φόρους βλ. View Φόρων)
                        //oRet.totalGrossValue = Math.Round((dTotal + _oDocument.TotalVATAmount), 2);//Net + taxes (Το Taxes περιλαμβάνει όλους τους επιπλέον φόρους βλ. View Φόρων)
                        //oRet.totalNetValue = dTotal;
                        //oRet.totalOtherTaxesAmount = 0.00;
                        //oRet.totalStampDutyAmount = 0.00;
                        //oRet.totalWithheldAmount = 0.00;
                        //oRet.expensesClassification = null;
                        #endregion
                        oRet.totalDeductionsAmount = Math.Round(dTotalDeductions, 2);
                        oRet.totalFeesAmount = Math.Round(dTotalFees, 2);
                        oRet.totalNetValue = Math.Round(dTotal, 2);
                        oRet.totalOtherTaxesAmount = Math.Round(dTotalOtherTaxes, 2);
                        oRet.totalStampDutyAmount = Math.Round(dTotalStamp, 2);
                        oRet.totalVatAmount = Math.Round((_oDocument.TotalVATAmount), 2);
                        //oRet.totalVatAmount = oRet.totalNetValue;
                        oRet.totalWithheldAmount = Math.Round(dTotalWithheldTaxes, 2);
                        oRet.totalGrossValue = Math.Round((dTotal + _oDocument.TotalVATAmount - dTotalDeductions - dTotalFees + dTotalStamp - dTotalOtherTaxes - dTotalWithheldTaxes), 2);//Net + taxes (Το Taxes περιλαμβάνει όλους τους επιπλέον φόρους βλ. View Φόρων)
                    }
                    _iResult++;
                }
                catch (Exception ex)
                {
                    var a = new Logging("myDataMethods.LoadnCreateClass.GetInvoiceSummary", ex);
                }
                return oRet;
            }


            /// <summary>
            /// Δημιουργία Αντικειμένου για την Εταιρεία που ανεβάζει
            /// </summary>
            /// <param name="_iResult">1 For Success, 0 For Failure</param>
            /// <returns>Το Αντικείμενο της ΑΑΔΕ για την Εταιρεία που Ανεβάζει</returns>
            private PartyType GetIssuer(out int _iResult, BoDocument _oDocument)
            {
                _iResult = 0;
                string sSQL = "";
                PartyType oRet = null;

                CommonLibrary.Ini.IniFile ini = new CommonLibrary.Ini.IniFile("C:\\Program Files\\SAP\\SAPmyDataService\\ConfParams.ini");
                string sCounterPart = ini.IniReadValue("Default", "EU_TX_COUNTER_ADDRESS");
                List<string> ListCounterpart = new List<string>();
                ListCounterpart = sCounterPart.Split(',').ToList();


                try
                {

                    if (this.CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_BRANCH WHERE 1=1 AND \"ObjType\" = '" + _oDocument.ObjType + "' AND \"DocEntry\" = '" + _oDocument.DocEntry + "'";
                    }
                    else
                    {
                        sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_BRANCH WHERE 1=1 AND ObjType = '" + _oDocument.ObjType + "' AND DocEntry = '" + _oDocument.DocEntry + "'";
                    }

                    SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, this.CompanyConnection);


                    oRet = new PartyType();
                    oRet.country = (CountryType)Enum.Parse(typeof(CountryType), oRS.Fields.Item("Country").Value.ToString());
                    oRet.branch = int.Parse(oRS.Fields.Item("Branch").Value.ToString());
                    oRet.vatNumber = oRS.Fields.Item("TaxIdNum").Value.ToString();

                    if ((_oDocument.ObjType.Equals("19") || _oDocument.ObjType.Equals("18")) && ListCounterpart.Contains(_oDocument.invoiceType) == true)
                    {
                        oRet.address = new AddressType();
                        oRet.address.postalCode = oRS.Fields.Item("ZipCode").Value.ToString();
                        oRet.address.city = oRS.Fields.Item("City").Value.ToString();
                        oRet.address.street = oRS.Fields.Item("Street").Value.ToString();
                        oRet.address.number = oRS.Fields.Item("StreetNo").Value.ToString();
                    }

                    _iResult++;
                }
                catch (Exception ex)
                {
                    var a = new Logging("myDataMethods.LoadnCreateClass.GetIssuer", ex);
                }
                return oRet;
            }

            /// <summary>
            /// Δημιουργία Αντικειμένου για τον Συν/μένο
            /// </summary>
            /// <param name="_iResult">1 For Success, 0 For Failure</param>
            /// <param name="_oDocument">To Αντικείμενο του Παραστατικού</param>
            /// <returns>Το Αντικείμενο της ΑΑΔΕ για τον Συν/μένο</returns>
            private PartyType GetCounterPart(BoDocument _oDocument, out int _iResult, InvoiceType _oInvoiceType)
            {
                _iResult = 0;
                PartyType oRet = null;
                try
                {
                    CommonLibrary.Ini.IniFile ini = new CommonLibrary.Ini.IniFile("C:\\Program Files\\SAP\\SAPmyDataService\\ConfParams.ini");
                    string sNoCounterPart = ini.IniReadValue("Default", "NO_COUNTERPART");
                    List<string> ListNoCounterpart = new List<string>();
                    ListNoCounterpart = sNoCounterPart.Split(',').ToList();

                    if (ListNoCounterpart.Contains(_oInvoiceType.ToString()) == false)
                    {
                        oRet = new PartyType();
                        switch (_oDocument.CounterPart_Define_Area)
                        {
                            case "GR":
                                string sNoAddress = ini.IniReadValue("Default", "GR_COUNTERPART_WITHOUT_ADDRESS");
                                List<string> ListNoAddress = new List<string>();
                                ListNoAddress = sNoAddress.Split(',').ToList();

                                string sNoName = ini.IniReadValue("Default", "GR_COUNTERPART_WITHOUT_NAME");
                                List<string> ListNoName = new List<string>();
                                ListNoName = sNoName.Split(',').ToList();

                                if (ListNoAddress.Contains(_oInvoiceType.ToString()) == true)
                                {
                                    oRet.vatNumber = _oDocument.CounterPart_vatNumber;
                                    oRet.country = (CountryType)Enum.Parse(typeof(CountryType), _oDocument.CounterPart_country);
                                    oRet.branch = int.Parse(_oDocument.CounterPart_branch);
                                }
                                else
                                {
                                    if (ListNoName.Contains(_oInvoiceType.ToString()) == false)
                                    {
                                        //if (_oInvoiceType != InvoiceType.Item71)
                                        //{
                                        oRet.name = _oDocument.CounterPart_name;
                                    }
                                    oRet.country = (CountryType)Enum.Parse(typeof(CountryType), _oDocument.CounterPart_country);
                                    oRet.vatNumber = _oDocument.CounterPart_vatNumber;
                                    oRet.branch = int.Parse(_oDocument.CounterPart_branch);
                                    oRet.address = new AddressType();
                                    oRet.address.city = _oDocument.CounterPart_country;
                                    oRet.address.street = _oDocument.CounterPart_address_street;
                                    oRet.address.postalCode = _oDocument.CounterPart_address_postalCode;
                                }
                                break;
                            case "EU":
                                oRet.name = _oDocument.CounterPart_name;
                                oRet.country = (CountryType)Enum.Parse(typeof(CountryType), _oDocument.CounterPart_country);
                                oRet.vatNumber = _oDocument.CounterPart_vatNumber;
                                oRet.branch = int.Parse(_oDocument.CounterPart_branch);
                                oRet.address = new AddressType();
                                oRet.address.city = _oDocument.CounterPart_country;
                                oRet.address.street = _oDocument.CounterPart_address_street;
                                oRet.address.postalCode = _oDocument.CounterPart_address_postalCode;
                                break;
                            case "TX":
                                oRet.name = _oDocument.CounterPart_name;
                                oRet.country = (CountryType)Enum.Parse(typeof(CountryType), _oDocument.CounterPart_country);
                                oRet.vatNumber = _oDocument.CounterPart_vatNumber;
                                oRet.branch = int.Parse(_oDocument.CounterPart_branch);
                                oRet.address = new AddressType();
                                oRet.address.city = _oDocument.CounterPart_address_city;
                                oRet.address.street = _oDocument.CounterPart_address_street;
                                oRet.address.postalCode = _oDocument.CounterPart_address_postalCode;
                                break;
                        }
                        _iResult++;
                    }
                    else
                    {
                        _iResult++;
                    }
                }
                catch (Exception ex)
                {
                    var a = new Logging("myDataMethods.LoadnCreateClass.GetCounterPart", ex);
                }
                return oRet;
            }

            /// <summary>
            /// Δημιουργία Αντικειμένου ΑΑΔΕ για τους Όρους Πληρωμής
            /// </summary>
            /// <param name="_oDocument">To Αντικείμενο του Παραστατικού</param>
            /// <param name="_iResult">1 For Success, 0 For Failure</param>
            /// <returns>Το Αντικείμενο της ΑΑΔΕ για τους Όρους Πληρωμής</returns>
            private List<PaymentMethodDetailType> GetPaymentMethods(BoDocument _oDocument, out int _iResult)
            {
                _iResult = 0;
                string sSQL = "";
                List<PaymentMethodDetailType> oRet = new List<PaymentMethodDetailType>();
                try
                {
                    if (this.CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_PAYMENT_TOTALS_WRAPPER WHERE 1=1 AND \"ObjType\" = '" + _oDocument.ObjType + "' AND \"DocEntry\" = '" + _oDocument.DocEntry + "'";
                    }
                    else
                    {
                        sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_PAYMENT_TOTALS_WRAPPER WHERE 1=1 AND ObjType = '" + _oDocument.ObjType + "' AND DocEntry = '" + _oDocument.DocEntry + "'";
                    }

                    SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, this.CompanyConnection);

                    while (oRS.EoF == false)
                    {
                        PaymentMethodDetailType oPayment = null;

                        oPayment = new PaymentMethodDetailType();
                        //oPayment.amount = double.Parse(oRS.Fields.Item("amount").Value.ToString());
                        oPayment.amount = Math.Round((double.Parse(oRS.Fields.Item("amount").Value.ToString())), 2);
                        oPayment.type = int.Parse(oRS.Fields.Item("type").Value.ToString());
                        oRet.Add(oPayment);

                        oRS.MoveNext();
                    }
                    _iResult++;
                }
                catch (Exception ex)
                {
                    Logging.WriteToLog("sSQL=" + sSQL, Logging.LogStatus.RET_VAL);
                    var a = new Logging("myDataMethods.LoadnCreateClass.GetPaymentMethods", ex);
                }
                return oRet;
            }

            /// <summary>
            /// Δημιουργία Header Δεδομένων ΑΑΔΕ Παραστατικού
            /// </summary>
            /// <param name="_oDocument">To Business Object</param>
            /// <param name="_iResult">1 For Success, 0 For Failure</param>
            /// <returns>Τον Header του Παραστατικού</returns>
            private InvoiceHeaderType GetInvoiceHeader(ref BoDocument _oDocument, out int _iResult)
            {
                _iResult = 0;
                InvoiceHeaderType oRet = new InvoiceHeaderType();
                string sSQL = "";
                try
                {
                    DateTime dtRefDate = DateTime.Now;

                    string sFileLocation = "C:\\Program Files\\SAP\\SAPmyDataService\\ConfParams.ini";
                    CommonLibrary.Ini.IniFile ini = new CommonLibrary.Ini.IniFile(sFileLocation);

                    //ΠΡΕΠΕΙ ΝΑ ΣΚΕΦΤΩ ΚΑΤΙ ΔΗΜΙΟΥΡΓΙΚΟ ΓΙΑ ΝΑ ΣΥΝΔΕΟΜΑΙ ΣΕ ΔΙΑΦΟΡΕΤΙΚΗ ΒΑΣΗ ΑΝΑΛΟΓΑ ΜΕ ΤΟ DBNAME (HANA EDITION)
                    string sConnectionString = ini.IniReadValue("Default", "MSSQLConnectionString");
                    sConnectionString = sConnectionString.Replace("#DB_NAME", _oDocument.CompanyDB);

                    if (this.CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_HEADER_WRAPPER WHERE 1=1 AND \"ObjType\" = '" + _oDocument.ObjType + "' AND \"DocEntry\" = '" + _oDocument.DocEntry + "'";
                    }
                    else
                    {
                        sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_HEADER_WRAPPER WHERE 1=1 AND ObjType = '" + _oDocument.ObjType + "' AND DocEntry = '" + _oDocument.DocEntry + "'";
                    }

                    SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, this.CompanyConnection);

                    while (oRS.EoF == false)
                    {
                        #region CounterPart Data
                        _oDocument.CounterPart_address_city = oRS.Fields.Item("CounterPart_address_city").Value.ToString();
                        _oDocument.CounterPart_address_postalCode = oRS.Fields.Item("CounterPart_address_postalCode").Value.ToString();
                        _oDocument.CounterPart_address_street = oRS.Fields.Item("CounterPart_address_street").Value.ToString();
                        _oDocument.CounterPart_branch = oRS.Fields.Item("CounterPart_branch").Value.ToString();
                        _oDocument.CounterPart_country = oRS.Fields.Item("CounterPart_country").Value.ToString();
                        _oDocument.CounterPart_name = oRS.Fields.Item("CounterPart_name").Value.ToString();
                        _oDocument.CounterPart_vatNumber = oRS.Fields.Item("CounterPart_vatNumber").Value.ToString();
                        _oDocument.CounterPart_Define_Area = oRS.Fields.Item("CounterPart_Define_Area").Value.ToString();

                        #endregion

                        #region Required
                        //oRet.aa = sTransID;
                        oRet.aa = oRS.Fields.Item("aa").Value.ToString();
                        oRet.series = oRS.Fields.Item("series").Value.ToString();
                        oRet.issueDate = DateTime.Parse(oRS.Fields.Item("issueDate").Value.ToString());
                        string invoiceType = oRS.Fields.Item("invoiceType").Value.ToString().Replace(".", "");
                        if (!string.IsNullOrEmpty(invoiceType))
                        {
                            oRet.invoiceType = (InvoiceType)Enum.Parse(typeof(InvoiceType), invoiceType);
                            _oDocument.invoiceType = oRet.invoiceType.ToString();
                        }
                        #endregion

                        #region NotRequired
                        if (string.IsNullOrEmpty(oRS.Fields.Item("currency").Value.ToString()) || oRS.Fields.Item("currency").Value.ToString().Equals("-112"))
                        {
                            oRet.currencySpecified = false;
                        }
                        else
                        {
                            CurrencyType enCur = (CurrencyType)Enum.Parse(typeof(CurrencyType), oRS.Fields.Item("currency").Value.ToString());
                            oRet.currencySpecified = true;
                            oRet.currency = enCur;
                        }

                        if (string.IsNullOrEmpty(oRS.Fields.Item("vatPaymentSuspension").Value.ToString()) || oRS.Fields.Item("vatPaymentSuspension").Value.ToString().Equals("-112"))
                        {
                            oRet.vatPaymentSuspensionSpecified = false;
                        }
                        else
                        {
                            oRet.vatPaymentSuspensionSpecified = true;
                            oRet.vatPaymentSuspension = oRS.Fields.Item("vatPaymentSuspension").Value.ToString();
                        }
                        if (string.IsNullOrEmpty(oRS.Fields.Item("exchangeRate").Value.ToString()) || double.Parse(oRS.Fields.Item("exchangeRate").Value.ToString()) == -775)
                        {
                            oRet.exchangeRateSpecified = false;
                        }
                        else
                        {
                            oRet.exchangeRateSpecified = true;
                            //oRet.exchangeRate = double.Parse(dtRow["exchangeRate"].ToString());
                            oRet.exchangeRate = Math.Round((double.Parse(oRS.Fields.Item("exchangeRate").Value.ToString())), 2);
                        }
                        if (string.IsNullOrEmpty(oRS.Fields.Item("selfPricing").Value.ToString()) || oRS.Fields.Item("selfPricing").Value.ToString().Equals("-112"))
                        {
                            oRet.selfPricingSpecified = false;
                        }
                        else
                        {
                            oRet.selfPricingSpecified = true;
                            oRet.selfPricing = oRS.Fields.Item("selfPricing").Value.ToString() == "0" ? false : true;
                        }
                        if (string.IsNullOrEmpty(oRS.Fields.Item("dispatchDate").Value.ToString()) || oRS.Fields.Item("dispatchDate").Value.ToString("yyyyMMdd").Equals("19000101"))
                        {
                            oRet.dispatchDateSpecified = false;
                        }
                        else
                        {
                            oRet.dispatchDateSpecified = true;
                            oRet.dispatchDate = DateTime.Parse(oRS.Fields.Item("dispatchDate").Value.ToString());
                        }
                        if (string.IsNullOrEmpty(oRS.Fields.Item("dispatchTime").Value.ToString()) || oRS.Fields.Item("dispatchTime").Value.ToString().Equals("-112"))
                        {
                            oRet.dispatchTimeSpecified = false;
                        }
                        else
                        {
                            oRet.dispatchTimeSpecified = true;
                            oRet.dispatchTime = DateTime.Parse(oRS.Fields.Item("dispatchTime").Value.ToString());
                        }
                        if (string.IsNullOrEmpty(oRS.Fields.Item("vehicleNumber").Value.ToString()) || oRS.Fields.Item("vehicleNumber").Value.ToString().Equals("-112"))
                        {
                            oRet.vehicleNumberSpecified = false;
                        }
                        else
                        {
                            oRet.vehicleNumber = oRS.Fields.Item("vehicleNumber").Value.ToString();
                            oRet.vehicleNumberSpecified = true;
                        }


                        string sNoMovePurpose = ini.IniReadValue("Default", "NO_MOVE_PURPOSE");
                        List<string> ListNoMovePurpose = new List<string>();
                        ListNoMovePurpose = sNoMovePurpose.Split(',').ToList();

                        //if (ListNoMovePurpose.Contains(_oInvoiceType.ToString()) == false)
                        //{
                        if (ListNoMovePurpose.Contains(oRet.invoiceType.ToString()) == true || string.IsNullOrEmpty(oRS.Fields.Item("movePurpose").Value.ToString()) || int.Parse(oRS.Fields.Item("movePurpose").Value.ToString()) == -112)
                        {
                            oRet.movePurposeSpecified = false;
                        }
                        else
                        {
                            oRet.movePurposeSpecified = true;
                            oRet.movePurpose = int.Parse(oRS.Fields.Item("movePurpose").Value.ToString());
                        }

                        if (!string.IsNullOrEmpty(oRS.Fields.Item("invoiceVariationType").Value.ToString()) && int.Parse(oRS.Fields.Item("invoiceVariationType").Value.ToString()) != -112)
                        {
                            oRet.invoiceVariationType = int.Parse(oRS.Fields.Item("invoiceVariationType").Value.ToString());
                            oRet.invoiceVariationTypeSpecified = true;
                        }
                        else
                        {
                            oRet.invoiceVariationTypeSpecified = false;
                        }

                        /*
                        if (oRS.Fields.Item("correlatedInvoices").Value!=null && !string.IsNullOrEmpty(oRS.Fields.Item("correlatedInvoices").Value.ToString()) && !oRS.Fields.Item("correlatedInvoices").Value.ToString().Equals("-112"))
                        {
                            string sCorrelatedMarks = oRS.Fields.Item("correlatedInvoices").Value.ToString();
                            oRet.correlatedInvoices = Array.ConvertAll(sCorrelatedMarks.Split(','), long.Parse).ToList();
                        }
                        */


                        //}
                        #endregion
                        //TODO
                        //List<long> correlatedInvoicesField;
                        oRS.MoveNext();
                    }

                    _iResult++;
                }
                catch (Exception ex)
                {
                    Logging.WriteToLog("sSQL=" + sSQL, Logging.LogStatus.RET_VAL);
                    var a = new Logging("myDataMethods.LoadnCreateClass.GetInvoiceHeader", ex);
                }
                return oRet;
            }

            private void SetIgnoreDue2Error(BoDocument _oDocument)
            {
                try
                {
                    DAL.BoUpdateDB oLog = new DAL.BoUpdateDB();
                    oLog.DocumentAA = _oDocument.DocumentAA;
                    oLog.Company = _oDocument.CompanyDB;
                    oLog.ObjType = _oDocument.ObjType;
                    oLog.DocEntry = _oDocument.DocEntry;
                    oLog.DocNum = _oDocument.DocNum;
                    oLog.isExpense = _oDocument.updateExpenses;
                    int iResult = oLog.UpdateDocumentSETIgnore(CompanyConnection);
                }
                catch (Exception ex)
                {
                    var a = new Logging("myDataMethods.SetIgnoreDue2Error", ex);
                }
            }
            #endregion

            #region Public Methods
            public int Exec(Enumerators.ot_Object _enType)
            {
                int iRetVal = 0;
                try
                {
                    int iSuccess = 2;
                    int iResult = 0;

                    //Logging.WriteToLog("myDataMethods.LoadnCreateClass.LoadDocumentsProcess", Logging.LogStatus.START);
                    iResult += this.LoadDocumentsProcess();
                    //Logging.WriteToLog("myDataMethods.LoadnCreateClass.LoadDocumentsProcess", Logging.LogStatus.END);

                    if (iResult == 1)
                    {
                        //Logging.WriteToLog("myDataMethods.LoadnCreateClass.PrepareDocumentsProcess", Logging.LogStatus.START);
                        iResult += this.PrepareDocumentsProcess();
                        //Logging.WriteToLog("myDataMethods.LoadnCreateClass.PrepareDocumentsProcess", Logging.LogStatus.END);
                    }

                    if (iResult == iSuccess)
                    {
                        iRetVal++;
                    }
                }
                catch (Exception ex)
                {
                    var a = new Logging("myDataMethods.LoadnCreateClass.Exec", ex);
                }
                return iRetVal;
            }
            #endregion
        }


        internal class LoadnCreateClassCancel
        {
            public List<BoDocument> ListDocumentsCancel { get; set; }
            public SAPbobsCOM.Company CompanyConnectionCancel { get; set; }

            public int returnsRows { get; set; }

            public LoadnCreateClassCancel()
            {
                this.ListDocumentsCancel = new List<BoDocument>();
            }

            #region Public Methods
            public int Exec(Enumerators.ot_Object _enType)
            {
                int iRetVal = 0;
                try
                {
                    int iSuccess = 1;
                    int iResult = 0;

                    //Logging.WriteToLog("myDataMethods.LoadnCreateClass.LoadDocumentsProcess", Logging.LogStatus.START);
                    iResult += this.LoadDocumentsCancelProcess();
                    //Logging.WriteToLog("myDataMethods.LoadnCreateClass.LoadDocumentsProcess", Logging.LogStatus.END);

                    if (iResult == iSuccess)
                    {
                        iRetVal++;
                    }
                }
                catch (Exception ex)
                {
                    var a = new Logging("myDataMethods.LoadnCreateClassCancel.Exec", ex);
                }
                return iRetVal;
            }
            #endregion

            #region Private Methods
            private int LoadDocumentsCancelProcess()
            {
                string sSQL = "";
                int iRetVal = 0;
                try
                {
                    this.ListDocumentsCancel = new List<BoDocument>();
                    BoDocument oDocument = null;

                    if (CompanyConnectionCancel.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                    {
                        sSQL = "SELECT * FROM TKA_V_SELECT_DOCUMENTS_2_CANCEL WHERE 1=1 ORDER BY AA DESC";
                    }
                    else
                    {
                        sSQL = "SELECT * FROM TKA_V_SELECT_DOCUMENTS_2_CANCEL WHERE 1=1 ORDER BY AA DESC";
                    }

                    SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, this.CompanyConnectionCancel);

                    while (oRS.EoF == false)
                    {
                        this.returnsRows = oRS.RecordCount;
                        oDocument = new BoDocument();
                        oDocument.DocumentAA = oRS.Fields.Item("AA").Value.ToString();
                        oDocument.CompanyDB = oRS.Fields.Item("COMPANY_DB").Value.ToString();
                        oDocument.ObjType = oRS.Fields.Item("OBJTYPE").Value.ToString();
                        oDocument.DocEntry = oRS.Fields.Item("DOCENTRY").Value.ToString();
                        oDocument.DocNum = oRS.Fields.Item("DOCNUM").Value.ToString();
                        oDocument.MARK = oRS.Fields.Item("MARK").Value.ToString();
                        oDocument.isExpense = int.Parse(oRS.Fields.Item("ISEXPENSE").Value.ToString());
                        if (oDocument.isExpense == 1)
                        {
                            oDocument.DocumentType = Enumerators.DocumentType.p_EU_TX;
                        }
                        else
                        {
                            oDocument.DocumentType = Enumerators.DocumentType.p_Income;
                        }

                        this.ListDocumentsCancel.Add(oDocument);
                        oRS.MoveNext();
                        //iResult+=this.LoadDocuments()
                    }

                    iRetVal++;
                }
                catch (Exception ex)
                {
                    Logging.WriteToLog("_sSQL=" + sSQL, Logging.LogStatus.RET_VAL);
                    var a = new Logging("myDataMethods.LoadnCreateClassCancel.LoadDocumentsCancelProcess", ex);
                }
                return iRetVal;
            }
            #endregion

        }

        #endregion
    }
}
