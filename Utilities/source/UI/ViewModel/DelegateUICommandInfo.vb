
Imports System


Namespace UI.ViewModel
    
    ''' <summary> Encapsulates all information needed to create a <see cref="DelegateUICommand(Of T1, T2)"/>, as there are delegates and UI decoration. </summary>
     ''' <typeparam name="T1"> Parameter type for CanExecute() predicate. </typeparam>
     ''' <typeparam name="T2"> Parameter type for Execute() action. </typeparam>
    Public Class DelegateUICommandInfo(Of T1, T2)
        
        ''' <summary> UI Decoration information. </summary>
        Public Decoration           As UICommandDecoration = Nothing
        
        ''' <summary> The execution logic. </summary>
        Public ExecuteAction        As Action(Of T2) = Nothing
        
        ''' <summary> The execution status logic. </summary>
        Public CanExecutePredicate  As Func(Of T1, Boolean) = Nothing
    
    End Class
    
    ''' <summary> A shortcut to create a <see cref="DelegateUICommand(Of Object, Object)"/>. </summary>
    Public Class DelegateUICommandInfo
        Inherits DelegateUICommandInfo(Of Object, Object)
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
