Imports System
Imports System.Collections.Generic
Imports System.Text

Namespace Validation
	''' <summary>
	''' A class to define a validation rule, pretty much the same as <see cref="Cinch.SimpleRule"/>, 
	''' except the delegate for validation can change the rule's broken description.
	''' </summary>
	''' <remarks>
	''' Recommended usage inside <see cref="Cinch.ValidatingObject"/>:
	''' <code>
	'''     //STEP 1:
	'''     //Declare a static field for the rule, so it is shared for all instance of the
	'''     //same type
	'''     Private Shared FieldNameRule As DelegateRule
	'''     
	'''     //STEP 2:
	'''     //Set the rule up in the static constructor
	'''     Shared Sub New()
	'''        
	'''        FieldNameRule = new DelegateRule("FieldName",
	'''                                         "Invalid Data Field",
	'''                                         Function (domainObject As Object, byRef BrokenDescription As String)
	'''                                             obj As MyObject = DirectCast(domainObject, MyObject)
	'''                                             Dim IsValid As Boolean = IsValidField()
	'''                                             BrokenDescription = "Found Field 'NameXXX' is not supported."
	'''                                             Return IsValid
	'''                                         End Function
	'''                                        )
	'''     End Sub
	'''     
	'''     //STEP 3:
	'''     //Add the rule in the instance constructor.
	'''     Public Sub New()
	'''         Me.AddRule(quantityRule)
	'''     End Sub
	''' </code>
	''' </remarks>
	Public Class DelegateRule
		Inherits Cinch.Rule
		
		#Region "Validator Delegate"
            
            ''' <summary> Represents the validation method used by <see cref="DelegateRule"/>. </summary>
             ''' <param name="ValidatingObject"> [Input]  The object to validate. </param>
             ''' <param name="BrokenMessage">    [Output] Returns a message indicating why rule is broken. </param>
 		    ''' <returns> <see langword="true"/> if the rule has been broken, otherwise <see langword="false"/>.</returns>
            Public Delegate Function RuleValidator(ValidatingObject As Object, ByRef BrokenMessage As String) As Boolean
		    
		#End Region
		
		#Region "Private Fields"
		    
		    Private ReadOnly _DefaultBrokenDescription  As String = Nothing
		    Private _CurentBrokenDescription            As String = Nothing
		    
		#End Region
		
		#Region "Constructors"
		    
		    ''' <summary> Creates a new Rule. </summary>
		    ''' <param name="PropertyName">             The name of the property this rule validates for. This may be blank. </param>
		    ''' <param name="DefaultBrokenDescription"> A default description message to show if the rule has been broken but <paramref name="RuleDelegate"/> hasn't created a message. </param>
		    ''' <param name="RuleDelegate">             A delegate to validate the rule. It takes the Object to validate as parameter, outputs a String message parameter and returns a boolean value. </param>
		    Public Sub New(PropertyName As String, DefaultBrokenDescription As String, RuleDelegate As RuleValidator)
		    	MyBase.New(PropertyName, DefaultBrokenDescription)
		    	
		    	_DefaultBrokenDescription = DefaultBrokenDescription
		    	Me.RuleDelegate = RuleDelegate
		    End Sub
		    
		#End Region
		
		#Region "Public Methods/Properties"
		    
		    ''' <summary> Gets or sets the delegate used to validate this rule. </summary>
            ''' <remarks> The delegate takes the Object to validate as parameter, outputs a String message parameter and returns a boolean value. </remarks>
		    Protected Overridable Property RuleDelegate() As RuleValidator
		    
		#End Region
		
		#Region "Overrides"
		    
		    ''' <summary>
		    ''' Validates that the rule has been broken.
		    ''' </summary>
		    ''' <param name="domainObject"> The domain object being validated. </param>
		    ''' <returns> <see langword="true"/> if the rule has been broken, otherwise <see langword="false"/>.</returns>
		    Public Overrides Function ValidateRule(domainObject As [Object]) As Boolean
		        
		    	Dim IsBroken As Boolean = RuleDelegate(domainObject, _CurentBrokenDescription)
		    	
		    	If (_CurentBrokenDescription.IsNotEmptyOrWhiteSpace()) Then
		    	    MyBase.Description = _CurentBrokenDescription
		    	Else
		    	    MyBase.Description = _DefaultBrokenDescription
		    	End If
		        
		    	Return IsBroken
		    End Function
		    
		#End Region
        
	End Class
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
