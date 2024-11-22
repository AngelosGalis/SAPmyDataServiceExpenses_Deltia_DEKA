﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=4.8.3928.0.
// 


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="https://www.aade.gr/myDATA/expensesClassificaton/v1.0")]
[System.Xml.Serialization.XmlRootAttribute(Namespace="https://www.aade.gr/myDATA/expensesClassificaton/v1.0", IsNullable=false)]
public partial class ExpensesClassificationsDoc {
    
    private InvoiceExpensesClassificationType[] expensesInvoiceClassificationField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("expensesInvoiceClassification")]
    public InvoiceExpensesClassificationType[] expensesInvoiceClassification {
        get {
            return this.expensesInvoiceClassificationField;
        }
        set {
            this.expensesInvoiceClassificationField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="https://www.aade.gr/myDATA/expensesClassificaton/v1.0")]
public partial class InvoiceExpensesClassificationType {
    
    private long invoiceMarkField;
    
    private long classificationMarkField;
    
    private bool classificationMarkFieldSpecified;
    
    private string entityVatNumberField;
    
    private object[] itemsField;
    
    /// <remarks/>
    public long invoiceMark {
        get {
            return this.invoiceMarkField;
        }
        set {
            this.invoiceMarkField = value;
        }
    }
    
    /// <remarks/>
    public long classificationMark {
        get {
            return this.classificationMarkField;
        }
        set {
            this.classificationMarkField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool classificationMarkSpecified {
        get {
            return this.classificationMarkFieldSpecified;
        }
        set {
            this.classificationMarkFieldSpecified = value;
        }
    }
    
    /// <remarks/>
    public string entityVatNumber {
        get {
            return this.entityVatNumberField;
        }
        set {
            this.entityVatNumberField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("invoicesExpensesClassificationDetails", typeof(InvoicesExpensesClassificationDetailType))]
    [System.Xml.Serialization.XmlElementAttribute("transactionMode", typeof(int))]
    public object[] Items {
        get {
            return this.itemsField;
        }
        set {
            this.itemsField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="https://www.aade.gr/myDATA/expensesClassificaton/v1.0")]
public partial class InvoicesExpensesClassificationDetailType {
    
    private int lineNumberField;
    
    private ExpensesClassificationType[] expensesClassificationDetailDataField;
    
    /// <remarks/>
    public int lineNumber {
        get {
            return this.lineNumberField;
        }
        set {
            this.lineNumberField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("expensesClassificationDetailData")]
    public ExpensesClassificationType[] expensesClassificationDetailData {
        get {
            return this.expensesClassificationDetailDataField;
        }
        set {
            this.expensesClassificationDetailDataField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="https://www.aade.gr/myDATA/expensesClassificaton/v1.0")]
public partial class ExpensesClassificationType {
    
    private ExpensesClassificationTypeClassificationType classificationTypeField;
    
    private bool classificationTypeFieldSpecified;
    
    private ExpensesClassificationCategoryType classificationCategoryField;
    
    private bool classificationCategoryFieldSpecified;
    
    private decimal amountField;
    
    private decimal vatAmountField;
    
    private bool vatAmountFieldSpecified;
    
    private int vatCategoryField;
    
    private bool vatCategoryFieldSpecified;
    
    private int vatExemptionCategoryField;
    
    private bool vatExemptionCategoryFieldSpecified;
    
    private sbyte idField;
    
    private bool idFieldSpecified;
    
    /// <remarks/>
    public ExpensesClassificationTypeClassificationType classificationType {
        get {
            return this.classificationTypeField;
        }
        set {
            this.classificationTypeField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool classificationTypeSpecified {
        get {
            return this.classificationTypeFieldSpecified;
        }
        set {
            this.classificationTypeFieldSpecified = value;
        }
    }
    
    /// <remarks/>
    public ExpensesClassificationCategoryType classificationCategory {
        get {
            return this.classificationCategoryField;
        }
        set {
            this.classificationCategoryField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool classificationCategorySpecified {
        get {
            return this.classificationCategoryFieldSpecified;
        }
        set {
            this.classificationCategoryFieldSpecified = value;
        }
    }
    
    /// <remarks/>
    public decimal amount {
        get {
            return this.amountField;
        }
        set {
            this.amountField = value;
        }
    }
    
    /// <remarks/>
    public decimal vatAmount {
        get {
            return this.vatAmountField;
        }
        set {
            this.vatAmountField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool vatAmountSpecified {
        get {
            return this.vatAmountFieldSpecified;
        }
        set {
            this.vatAmountFieldSpecified = value;
        }
    }
    
    /// <remarks/>
    public int vatCategory {
        get {
            return this.vatCategoryField;
        }
        set {
            this.vatCategoryField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool vatCategorySpecified {
        get {
            return this.vatCategoryFieldSpecified;
        }
        set {
            this.vatCategoryFieldSpecified = value;
        }
    }
    
    /// <remarks/>
    public int vatExemptionCategory {
        get {
            return this.vatExemptionCategoryField;
        }
        set {
            this.vatExemptionCategoryField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool vatExemptionCategorySpecified {
        get {
            return this.vatExemptionCategoryFieldSpecified;
        }
        set {
            this.vatExemptionCategoryFieldSpecified = value;
        }
    }
    
    /// <remarks/>
    public sbyte id {
        get {
            return this.idField;
        }
        set {
            this.idField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool idSpecified {
        get {
            return this.idFieldSpecified;
        }
        set {
            this.idFieldSpecified = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="https://www.aade.gr/myDATA/expensesClassificaton/v1.0")]
public enum ExpensesClassificationTypeClassificationType {
    
    /// <remarks/>
    E3_101,
    
    /// <remarks/>
    E3_102_001,
    
    /// <remarks/>
    E3_102_002,
    
    /// <remarks/>
    E3_102_003,
    
    /// <remarks/>
    E3_102_004,
    
    /// <remarks/>
    E3_102_005,
    
    /// <remarks/>
    E3_102_006,
    
    /// <remarks/>
    E3_104,
    
    /// <remarks/>
    E3_201,
    
    /// <remarks/>
    E3_202_001,
    
    /// <remarks/>
    E3_202_002,
    
    /// <remarks/>
    E3_202_003,
    
    /// <remarks/>
    E3_202_004,
    
    /// <remarks/>
    E3_202_005,
    
    /// <remarks/>
    E3_204,
    
    /// <remarks/>
    E3_207,
    
    /// <remarks/>
    E3_209,
    
    /// <remarks/>
    E3_301,
    
    /// <remarks/>
    E3_302_001,
    
    /// <remarks/>
    E3_302_002,
    
    /// <remarks/>
    E3_302_003,
    
    /// <remarks/>
    E3_302_004,
    
    /// <remarks/>
    E3_302_005,
    
    /// <remarks/>
    E3_304,
    
    /// <remarks/>
    E3_307,
    
    /// <remarks/>
    E3_309,
    
    /// <remarks/>
    E3_312,
    
    /// <remarks/>
    E3_313_001,
    
    /// <remarks/>
    E3_313_002,
    
    /// <remarks/>
    E3_313_003,
    
    /// <remarks/>
    E3_313_004,
    
    /// <remarks/>
    E3_313_005,
    
    /// <remarks/>
    E3_315,
    
    /// <remarks/>
    E3_581_001,
    
    /// <remarks/>
    E3_581_002,
    
    /// <remarks/>
    E3_581_003,
    
    /// <remarks/>
    E3_582,
    
    /// <remarks/>
    E3_583,
    
    /// <remarks/>
    E3_584,
    
    /// <remarks/>
    E3_585_001,
    
    /// <remarks/>
    E3_585_002,
    
    /// <remarks/>
    E3_585_003,
    
    /// <remarks/>
    E3_585_004,
    
    /// <remarks/>
    E3_585_005,
    
    /// <remarks/>
    E3_585_006,
    
    /// <remarks/>
    E3_585_007,
    
    /// <remarks/>
    E3_585_008,
    
    /// <remarks/>
    E3_585_009,
    
    /// <remarks/>
    E3_585_010,
    
    /// <remarks/>
    E3_585_011,
    
    /// <remarks/>
    E3_585_012,
    
    /// <remarks/>
    E3_585_013,
    
    /// <remarks/>
    E3_585_014,
    
    /// <remarks/>
    E3_585_015,
    
    /// <remarks/>
    E3_585_016,
    
    /// <remarks/>
    E3_586,
    
    /// <remarks/>
    E3_587,
    
    /// <remarks/>
    E3_588,
    
    /// <remarks/>
    E3_589,
    
    /// <remarks/>
    E3_881_001,
    
    /// <remarks/>
    E3_881_002,
    
    /// <remarks/>
    E3_881_003,
    
    /// <remarks/>
    E3_881_004,
    
    /// <remarks/>
    E3_882_001,
    
    /// <remarks/>
    E3_882_002,
    
    /// <remarks/>
    E3_882_003,
    
    /// <remarks/>
    E3_882_004,
    
    /// <remarks/>
    E3_883_001,
    
    /// <remarks/>
    E3_883_002,
    
    /// <remarks/>
    E3_883_003,
    
    /// <remarks/>
    E3_883_004,
    
    /// <remarks/>
    VAT_361,
    
    /// <remarks/>
    VAT_362,
    
    /// <remarks/>
    VAT_363,
    
    /// <remarks/>
    VAT_364,
    
    /// <remarks/>
    VAT_365,
    
    /// <remarks/>
    VAT_366,
    
    /// <remarks/>
    E3_103,
    
    /// <remarks/>
    E3_203,
    
    /// <remarks/>
    E3_303,
    
    /// <remarks/>
    E3_208,
    
    /// <remarks/>
    E3_308,
    
    /// <remarks/>
    E3_314,
    
    /// <remarks/>
    E3_106,
    
    /// <remarks/>
    E3_205,
    
    /// <remarks/>
    E3_305,
    
    /// <remarks/>
    E3_210,
    
    /// <remarks/>
    E3_310,
    
    /// <remarks/>
    E3_318,
    
    /// <remarks/>
    E3_598_002,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="https://www.aade.gr/myDATA/expensesClassificaton/v1.0")]
public enum ExpensesClassificationCategoryType {
    
    /// <remarks/>
    category2_1,
    
    /// <remarks/>
    category2_2,
    
    /// <remarks/>
    category2_3,
    
    /// <remarks/>
    category2_4,
    
    /// <remarks/>
    category2_5,
    
    /// <remarks/>
    category2_6,
    
    /// <remarks/>
    category2_7,
    
    /// <remarks/>
    category2_8,
    
    /// <remarks/>
    category2_9,
    
    /// <remarks/>
    category2_10,
    
    /// <remarks/>
    category2_11,
    
    /// <remarks/>
    category2_12,
    
    /// <remarks/>
    category2_13,
    
    /// <remarks/>
    category2_14,
    
    /// <remarks/>
    category2_95,
}
