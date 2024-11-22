using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary.ExceptionHandling;
using Response;
using SAPmyDataService.Enumerators;
using SAPbobsCOM;

namespace SAPmyDataService.BusinessLayer
{
    public class BoDocument
    {
        #region Public Properties
        public string DocumentAA { get; set; }
        public string ObjType { get; set; }
        public string DocEntry { get; set; }
        public string DocNum { get; set; }
        public bool isDelivery { get; set; }
        public string MARK { get; set; }
        public string ClassMark { get; set; }
        public string UID { get; set; }
        public string QR { get; set; }
        public string CompanyDB { get; set; }
        public string TransId { get; set; }
        public int isExpense { get; set; }
        public int updateExpenses { get; set; }
        public string invoiceType { get; set; }

        public int useNewMethod { get; set; }

        public string reject_deviation { get; set; }
        public double TotalVATAmount { get; set; }
        public double TotalTaxesAmount { get; set; }

        public string CounterPart_name { get; set; }
        public string CounterPart_vatNumber { get; set; }
        public string CounterPart_country { get; set; }
        public string CounterPart_branch { get; set; }
        public string CounterPart_address_city { get; set; }
        public string CounterPart_address_street { get; set; }
        public string CounterPart_address_postalCode { get; set; }
        public string CounterPart_Define_Area { get; set; }
        public string CounterPart_Number { get; set; }

        public string deliveryVatNumber { get; set; }

        public DocumentPrepared DocumentStatus { get; set; }
        public InvoicesDoc AADEDocument { get; set; }

        public DocumentType DocumentType {get;set;}

        public string StatusCode { get; set; }
        public SAPResult Result { get; set; }
        public ExpensesClassificationsDoc AADEMatchingDocument{get; set; }
        public IncomeClassificationsDoc AADEMatchingDocumentIncome { get; set; }
        #endregion
        public BoDocument()
        {
            this.DocumentStatus = DocumentPrepared.p_NA;
            this.AADEDocument = new InvoicesDoc();
        }

        #region Public Methods
        public int LoadCorrelatedDocuments()
        {
            int iRetVal = 0;
            string sSQL = "";
            try
            {
            }
            catch (Exception ex)
            {
                Logging.WriteToLog("sSQL" + sSQL, Logging.LogStatus.RET_VAL);
                var a = new Logging("BoDocument.LoadCorrelatedDocuments", ex);
            }
            return iRetVal;
        }


        public int LoadTotals(SAPbobsCOM.Company _oCompany)
        {
            int iRetVal = 0;
            try
            {
                this.LoadVATTotals(_oCompany);
                this.LoadTaxesTotals(_oCompany);
            }
            catch (Exception ex)
            {
                var a = new Logging("BoDocument.LoadTotals", ex);
            }
            return iRetVal;
        }

        public int DefineType()
        {
            int iRetVal = 0;
            try
            {
                if(this.isExpense==0 ) {
                    this.DocumentType = Enumerators.DocumentType.p_Income;
                }else if (this.isExpense == 1 && !string.IsNullOrEmpty(this.ObjType) && !string.IsNullOrEmpty(this.DocEntry) && !string.IsNullOrEmpty(this.MARK) && (this.reject_deviation.Equals("M") || this.reject_deviation.Equals("ERROR_306")))
                {
                    this.DocumentType = Enumerators.DocumentType.p_Matched;

                }
                else if (this.isExpense == 1 && !string.IsNullOrEmpty(this.ObjType) && !string.IsNullOrEmpty(this.DocEntry) && string.IsNullOrEmpty(this.MARK) && this.reject_deviation.Equals("-"))
                {
                    this.DocumentType = Enumerators.DocumentType.p_EU_TX;

                }
                else if (this.isExpense == 1 &&  string.IsNullOrEmpty(this.DocEntry) && !string.IsNullOrEmpty(this.MARK) && this.reject_deviation.Equals("R"))
                {
                    this.DocumentType = Enumerators.DocumentType.p_Reject;

                }
                else if (this.isExpense == 1 && string.IsNullOrEmpty(this.DocEntry) && !string.IsNullOrEmpty(this.MARK) && this.reject_deviation.Equals("D"))
                {
                    this.DocumentType = Enumerators.DocumentType.p_Deviation;

                }
            }
            catch (Exception ex)
            {
                var a = new Logging("BoDocument.DefineType", ex);
            }
            return iRetVal;
        }
        #endregion

        #region Private Properties
        private int LoadTaxesTotals(SAPbobsCOM.Company _oCompany)
        {
            int iRetVal = 0;
            string sSQL = "";
            try
            {
                //sSQL = "SELECT * FROM TKA_V_ELECTRONIC_INVOICES_VAT_TOTALS";
                this.TotalTaxesAmount = 0;
            }
            catch (Exception ex)
            {
                Logging.WriteToLog("sSQL" + sSQL, Logging.LogStatus.RET_VAL);
                var a = new Logging("BoDocument.LoadTaxesTotals", ex);
            }
            return iRetVal;
        }

        private int LoadVATTotals(SAPbobsCOM.Company _oCompany)
        {
            int iRetVal = 0;
            string sSQL = "";
            try
            {
                if (_oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
                {
                    sSQL = "SELECT \"vatAmount\" FROM TKA_V_ELECTRONIC_INVOICES_VAT_TOTALS_WRAPPER WHERE 1=1 AND \"ObjType\" = '" + this.ObjType + "' AND \"DocEntry\" = '" + this.DocEntry + "'";
                }
                else
                {
                    sSQL = "SELECT vatAmount FROM TKA_V_ELECTRONIC_INVOICES_VAT_TOTALS_WRAPPER WHERE 1=1 AND ObjType = '" + this.ObjType + "' AND DocEntry = '" + this.DocEntry + "'";
                }

                this.TotalVATAmount = Math.Round(double.Parse(CommonLibrary.Functions.Database.ReturnDBValues(sSQL, "vatAmount", _oCompany).ToString()), 2);

                iRetVal++;
            }
            catch (Exception ex)
            {
                Logging.WriteToLog("sSQL" + sSQL, Logging.LogStatus.RET_VAL);
                var a = new Logging("BoDocument.LoadVATTotals", ex);
            }
            return iRetVal;
        }
        #endregion
    }
}
