
Namespace Domain.ClearanceGauge
    
    ''' <summary> Supported scopes for automatic clearance definition. </summary>
    Public Enum ClearanceRulesScope As Integer
        
        ''' <summary> No scope determined. </summary>
        None = 0
        
        ''' <summary> Deutsche Bahn AG. </summary>
        DBAG = 1
        
    End Enum
    
    ''' <summary> Clearance definitions supported for special cases, i.e. for automatic computation. </summary>
    Public Enum ClearanceDefinition As Integer
        
        ''' <summary> No clearance determined. </summary>
        None = 0
        
        ''' <summary> The Clearance which has to be kept free in every case (Grenzlinie). </summary>
        MinimumClearance = 1
        
    End Enum
    
    ''' <summary> Basis of clearance definition. </summary>
    Public Enum ClearanceDefinitionBase As Integer
        
        ''' <summary> No basis determined. </summary>
        None = 0
        
        ''' <summary> Static definition by existing polygons in CAD. </summary>
        CAD = 1
        
        ''' <summary> Automatic definition by base line DBAG G2. </summary>
        G2 = 2
        
    End Enum
    
    ''' <summary> Supported optional clearance parts (for automatic definition). </summary>
    Public Enum ClearanceOptionalPart As Integer
        
        ''' <summary> No optional part determined. </summary>
        None = 0
        
        ''' <summary> Custom defined optional part. </summary>
        Custom = 1
        
        ''' <summary> Overhead line 1 kV. </summary>
        OHL_1kV = 2
        
        ''' <summary> Overhead line 3 kV. </summary>
        OHL_3kV = 3
        
        ''' <summary> Overhead line 15 kV. </summary>
        OHL_15kV = 4
        
        ''' <summary> Overhead line 25 kV. </summary>
        OHL_25kV = 5
        
    End Enum
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::tabSize=4:
