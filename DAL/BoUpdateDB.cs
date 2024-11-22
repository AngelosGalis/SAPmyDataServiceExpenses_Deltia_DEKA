using CommonLibrary.ExceptionHandling;
using SAPmyDataService.BusinessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPmyDataService.DAL
{
    public class BoUpdateDB
    {
        #region Public Properties
        public string DocumentAA { get; set; }
        public string Company { get; set; }
        public string ObjType { get; set; }
        public int Cancel { get; set; }
        public string DocEntry { get; set; }
        public string DocNum { get; set; }
        public string XMLReply { get; set; }
        public string MARK { get; set; }
        public string CANCELLED_MARK { get; set; }
        public string UID { get; set; }
        public string QR { get; set; }
        public string Result { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescr { get; set; }
        public int type { get; set; }
        public int isChecked { get; set; }
        public int isExpense { get; set; }

        #endregion

        public BoUpdateDB()
        {
            this.MARK = "";
            this.UID = "";
            this.XMLReply = "";
            this.Result = "";
            this.ErrorCode = "";
            this.ErrorDescr = "";
        }

        public int AddResponse(SAPbobsCOM.Company CompanyConnection)
        {
            int iRetVal = 0;
            string sSQL = "";
            try
            {
                string sFileLocation = "C:\\Program Files\\SAP\\SAPmyDataServiceDA\\ConfParams.ini";
                CommonLibrary.Ini.IniFile ini = new CommonLibrary.Ini.IniFile(sFileLocation);
                string sConnectionString = ini.IniReadValue("Default", "MSSQLConnectionString");
                if (CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    sSQL = "call \"RESPONSES_INSERT\"(" +
                        "'" + ObjType + "'," +
                        "'" + DocEntry + "'," +
                        "'" + DocNum + "'," +
                        "'" + XMLReply.Replace("'", "") + "'," +
                        "'" + Cancel
                        + "')";
                    SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, CompanyConnection);
                }
                else
                {
                    using (SqlConnection oConnection = new SqlConnection(sConnectionString))
                    {
                        oConnection.Open();

                        using (SqlCommand oCommand = new SqlCommand("[dbo].RESPONSES_INSERT", oConnection))
                        {
                            oCommand.CommandTimeout = 0;
                            oCommand.Parameters.Add(new SqlParameter("@ObjType", "" + this.ObjType + ""));
                            oCommand.Parameters.Add(new SqlParameter("@DocEntry", "" + this.DocEntry + ""));
                            oCommand.Parameters.Add(new SqlParameter("@DocNum", "" + this.DocNum + ""));
                            oCommand.Parameters.Add(new SqlParameter("@XMLReply", "" + this.XMLReply + ""));
                            oCommand.Parameters.Add(new SqlParameter("@Cancel", "" + this.Cancel + ""));

                            oCommand.CommandType = CommandType.StoredProcedure;

                            oCommand.ExecuteScalar();
                        }
                        oConnection.Close();
                    }
                }

                iRetVal++;
            }
            catch (Exception ex)
            {
                Logging.WriteToLog("sSQL=" + sSQL, Logging.LogStatus.RET_VAL);
                var a = new Logging("BoDAL.AddResponse", ex);
            }
            return iRetVal;
        }

        public int UpdateDocument(string _sTableName, SAPbobsCOM.Company CompanyConnection)
        {
            int iRetVal = 0;
            string sSQL = "";
            int iResult = 0;
            try
            {
                string sFileLocation = "C:\\Program Files\\SAP\\SAPmyDataServiceDA\\ConfParams.ini";
                CommonLibrary.Ini.IniFile ini = new CommonLibrary.Ini.IniFile(sFileLocation);
                string error_dscr = "";
                if (!string.IsNullOrEmpty(ErrorDescr))
                {
                    error_dscr = ErrorDescr.Replace("'", "");
                }
                if (CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    sSQL = "call \"" + _sTableName + "_UPDATE\"(" +
                        "'" + this.DocumentAA + "'," +
                        "'" + MARK + "'," +
                        "'" + UID + "'," +
                        "'" + QR + "'," +
                        "'" + Result + "'," +
                        "'" + ErrorCode + "'," +
                        "'" + error_dscr + "'";
                    if (_sTableName.Equals("EXPENSES"))
                    {
                        sSQL += ",'" + type + "'";
                        sSQL += ",'" + isChecked + "'";
                    }
                    sSQL += ")";
                    SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, CompanyConnection);
                    if (oRS != null)
                    {
                        iResult++;
                    }
                }
                else
                {
                    string sConnectionString = ini.IniReadValue("Default", "MSSQLConnectionString");
                    sSQL = "[dbo]." + _sTableName + "_UPDATE";
                    using (SqlConnection oConnection = new SqlConnection(sConnectionString))
                    {
                        oConnection.Open();

                        using (SqlCommand oCommand = new SqlCommand(sSQL, oConnection))
                        {
                            oCommand.CommandTimeout = 0;
                            oCommand.Parameters.Add(new SqlParameter("@DOCUMENT_AA", "" + this.DocumentAA + ""));
                            oCommand.Parameters.Add(new SqlParameter("@MARK", "" + this.MARK + ""));
                            oCommand.Parameters.Add(new SqlParameter("@UID", "" + this.UID + ""));
                            oCommand.Parameters.Add(new SqlParameter("@QR", "" + this.QR + ""));
                            oCommand.Parameters.Add(new SqlParameter("@RESULT", "" + this.Result + ""));
                            oCommand.Parameters.Add(new SqlParameter("@ERROR_CODE", "" + this.ErrorCode + ""));
                            oCommand.Parameters.Add(new SqlParameter("@ERROR_DESCR", "" + this.ErrorDescr + ""));

                            if (_sTableName.Equals("EXPENSES"))
                            {
                                oCommand.Parameters.Add(new SqlParameter("@checked", "" + this.isChecked + ""));
                                oCommand.Parameters.Add(new SqlParameter("@type", "" + this.type + ""));

                            }
                            oCommand.CommandType = CommandType.StoredProcedure;

                            oCommand.ExecuteScalar();
                        }
                        oConnection.Close();
                    }
                    iResult++;
                }

                if (iResult == 1)
                {
                    iRetVal++;
                }
            }
            catch (Exception ex)
            {
                Logging.WriteToLog("sSQL=" + sSQL, Logging.LogStatus.RET_VAL);
                var a = new Logging("BoDAL.UpdateDocument", ex);
            }
            return iRetVal;
        }


      

        public int UpdateDocument4Cancel(string _sTableName, SAPbobsCOM.Company CompanyConnection)
        {
            int iRetVal = 0;
            string sSQL = "";
            try
            {
                string sFileLocation = "C:\\Program Files\\SAP\\SAPmyDataServiceDA\\ConfParams.ini";
                CommonLibrary.Ini.IniFile ini = new CommonLibrary.Ini.IniFile(sFileLocation);
                if (CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    string error_dscr = "";
                    if (!string.IsNullOrEmpty(ErrorDescr))
                    {
                        error_dscr = ErrorDescr.Replace("'", "");
                    }
                    sSQL = "call \"" + _sTableName + "_UPDATE_CANCEL_FLOW\"(" +
                        "'" + DocumentAA + "'," +
                        "'" + CANCELLED_MARK + "'," +
                        "'" + Result + "'," +
                        "'" + ErrorCode + "'," +
                        "'" + error_dscr
                        + "')";
                    SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, CompanyConnection);
                }
                else
                {
                    string sConnectionString = ini.IniReadValue("Default", "MSSQLConnectionString");
                    sSQL = "[dbo]." + _sTableName + "_UPDATE_CANCEL_FLOW";
                    using (SqlConnection oConnection = new SqlConnection(sConnectionString))
                    {
                        oConnection.Open();

                        using (SqlCommand oCommand = new SqlCommand(sSQL, oConnection))
                        {
                            oCommand.CommandTimeout = 0;
                            oCommand.Parameters.Add(new SqlParameter("@DOCUMENT_AA", "" + this.DocumentAA + ""));
                            oCommand.Parameters.Add(new SqlParameter("@CANCELLED_MARK", "" + this.CANCELLED_MARK + ""));
                            oCommand.Parameters.Add(new SqlParameter("@RESULT", "" + this.Result + ""));
                            oCommand.Parameters.Add(new SqlParameter("@ERROR_CODE", "" + this.ErrorCode + ""));
                            oCommand.Parameters.Add(new SqlParameter("@ERROR_DESCR", "" + this.ErrorDescr + ""));
                            oCommand.CommandType = CommandType.StoredProcedure;

                            oCommand.ExecuteScalar();
                        }
                        oConnection.Close();
                    }
                }
                iRetVal++;
            }
            catch (Exception ex)
            {
                Logging.WriteToLog("sSQL=" + sSQL, Logging.LogStatus.RET_VAL);
                var a = new Logging("BoDAL.UpdateDocument4Cancel", ex);
            }
            return iRetVal;
        }
        public int UpdateDocumentSETIgnore(SAPbobsCOM.Company CompanyConnection)
        {
            int iRetVal = 0;
            string sSQL = "";
            try
            {


                if (CompanyConnection.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    if (this.isExpense == 1)
                    {
                        sSQL = "call \"EXPENSES_UPDATE_SET_IGNORE\"('" + this.DocumentAA + "')";
                    }
                    else
                    {
                        sSQL = "call \"DOCUMENTS_UPDATE_SET_IGNORE\"('" + this.DocumentAA + "')";
                    }


                    SAPbobsCOM.Recordset oRS = CommonLibrary.Functions.Database.GetRecordSet(sSQL, CompanyConnection);
                }
                else
                {
                    if (this.isExpense == 1)
                    {
                        sSQL = "[dbo].EXPENSES_UPDATE_SET_IGNORE";
                    }
                    else
                    {
                        sSQL = "[dbo].DOCUMENTS_UPDATE_SET_IGNORE";
                    }

                    string sFileLocation = "C:\\Program Files\\SAP\\SAPmyDataServiceDA\\ConfParams.ini";
                    CommonLibrary.Ini.IniFile ini = new CommonLibrary.Ini.IniFile(sFileLocation);

                    string sConnectionString = ini.IniReadValue("Default", "MSSQLConnectionString");

                    using (SqlConnection oConnection = new SqlConnection(sConnectionString))
                    {
                        oConnection.Open();

                        using (SqlCommand oCommand = new SqlCommand(sSQL, oConnection))
                        {
                            oCommand.CommandTimeout = 0;
                            oCommand.Parameters.Add(new SqlParameter("@DOCUMENT_AA", "" + this.DocumentAA + ""));
                            oCommand.CommandType = CommandType.StoredProcedure;

                            oCommand.ExecuteScalar();
                        }
                        oConnection.Close();
                    }
                }

                iRetVal++;
            }
            catch (Exception ex)
            {
                Logging.WriteToLog("sSQL=" + sSQL, Logging.LogStatus.RET_VAL);
                var a = new Logging("BoDAL.UpdateDocument", ex);
            }
            return iRetVal;
        }
    }
}
