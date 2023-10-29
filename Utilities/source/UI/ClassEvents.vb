
Imports System.Windows
Imports System.Windows.Input
Imports System.Windows.Controls

Namespace UI
    
    ''' <summary> Central class event handling. </summary>
    Public NotInheritable Class ClassEvents
        
        #Region "Private Fields"
            
            Private Shared ReadOnly Logger      As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger("Rstyx.Utilities.ClassEvents")
            
            Private Shared ReadOnly SyncHandle  As New Object()
            
        #End Region
        
        #Region "Constructor"
            
            Private Sub New
                'Hides the Constructor
            End Sub
            
	        Shared Sub New()
	            EventManager.RegisterClassHandler(GetType(TextBox), TextBox.GotKeyboardFocusEvent, New KeyboardFocusChangedEventHandler(AddressOf OnTextBoxGotKeyboardFocus))
	        End Sub
            
        #End Region
        
        #Region "Shared Properties"
	        
            Private Shared _SelectAllOnTextBoxGotFocus As Boolean = False
            
            ''' <summary> Determines whether or not all text of a text box should be selected, when it got keyboard focus. </summary>
	        Public Shared Property SelectAllOnTextBoxGotFocus() As Boolean
                Get
                    Return _SelectAllOnTextBoxGotFocus
                End Get
                Set(value As Boolean)
                    SyncLock (SyncHandle)
                        If (value Xor _SelectAllOnTextBoxGotFocus) Then
                            _SelectAllOnTextBoxGotFocus = value
                        End if
                    End SyncLock
                End Set
	        End Property
            
        #End Region
        
        #Region "Class Event Handler"
	        
            ''' <summary> Selects all text of a text box, if desired. </summary>
	        Private Shared Sub OnTextBoxGotKeyboardFocus(ByVal sender As Object, ByVal e As KeyboardFocusChangedEventArgs)
	            Try
                    If (_SelectAllOnTextBoxGotFocus) Then
	                    CType(sender, TextBox).SelectAll()
                    End If
                Catch ex As System.Exception
                    Logger.LogError(ex, Rstyx.Utilities.Resources.Messages.Global_ErrorFromInsideEventHandler)
                End Try
	        End Sub
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
