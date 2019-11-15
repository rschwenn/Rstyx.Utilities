
Imports System.Collections.Generic
Imports Rstyx.Utilities.StringUtils

Namespace Domain
    
    ''' <summary> One single iPoint. </summary>
    Public Class GeoIPoint
        Inherits GeoPoint
        
        #Region "Private Fields"
            
            Private Shared ReadOnly MaxIDLength     As Integer = 20
            Private Shared ReadOnly AttSeparator    As Char    = "|"
            
        #End Region
        
        #Region "Constuctor"
            
            ''' <summary> Creates a new, empty <see cref="GeoIPoint"/>. </summary>
            Public Sub New()
            End Sub
            
            ''' <summary> Creates a new <see cref="GeoIPoint"/> and inititializes it's properties from any given <see cref="IGeoPoint"/>. </summary>
             ''' <param name="SourcePoint"> The source point to get init values from. May be <see langword="null"/>. </param>
             ''' <remarks></remarks>
             ''' <exception cref="InvalidIDException"> ID of <paramref name="SourcePoint"/> isn't a valid ID for this point. </exception>
            Public Sub New(SourcePoint As IGeoPoint)
                MyBase.New(SourcePoint)
                
                Me.CoordType = "YXZ"
                
                ' iPkt specials.
                If (TypeOf SourcePoint Is GeoIPoint) Then
                    
                    Dim SourceIPoint As GeoIPoint = DirectCast(SourcePoint, GeoIPoint)
                    
                    Me.AttKey1     = SourceIPoint.AttKey1
                    Me.AttKey2     = SourceIPoint.AttKey2
                    Me.AttValue1   = SourceIPoint.AttValue1
                    Me.AttValue2   = SourceIPoint.AttValue2
                    Me.CalcCode    = SourceIPoint.CalcCode
                    Me.CoordType   = SourceIPoint.CoordType
                    Me.Flags       = SourceIPoint.Flags
                    Me.GraficsCode = SourceIPoint.GraficsCode
                    Me.GraficsDim  = SourceIPoint.GraficsDim
                    Me.GraficsEcc  = SourceIPoint.GraficsEcc
                    
                ElseIf (TypeOf SourcePoint Is GeoVEPoint) Then
                        
                    Dim SourceVEPoint As GeoVEPoint = DirectCast(SourcePoint, GeoVEPoint)
                        
                    'If (Me.ObjectKey.IsEmptyOrWhiteSpace() OrElse (Me.ObjectKey = "0")) Then
                    '    Me.ObjectKey = SourceIPoint.Kind
                    'End If
                    
                    ' Take VE specials as comment.
                    Me.Comment = sprintf(" %12.4f  %-13s %-4s %4d %1s  %3s %5.0f %5.0f  %1s %+3s  %1s%1s  %-8s  # %-s",
                                         SourceVEPoint.TrackPos.Kilometer.Value,
                                         SourceVEPoint.HeightInfo.TrimToMaxLength(13),
                                         SourceVEPoint.Kind.TrimToMaxLength(4),
                                         SourceVEPoint.TrackPos.TrackNo,
                                         SourceVEPoint.TrackPos.RailsCode.TrimToMaxLength(1),
                                         SourceVEPoint.HeightSys.TrimToMaxLength(3),
                                         SourceVEPoint.mp,
                                         SourceVEPoint.mh, 
                                         SourceVEPoint.MarkHints.TrimToMaxLength(1),
                                         SourceVEPoint.MarkType.TrimToMaxLength(3),
                                         SourceVEPoint.sp.TrimToMaxLength(1),
                                         SourceVEPoint.sh.TrimToMaxLength(1),
                                         SourceVEPoint.Job.TrimToMaxLength(8),
                                         SourceVEPoint.Comment
                                        )
                End If
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            ''' <summary> Calculation Code. </summary>
            Public Property CalcCode    As String = Nothing
            
            ''' <summary> Grafics Code. </summary>
            Public Property GraficsCode As String = Nothing
            
            ''' <summary> Grafics Dimension. </summary>
            Public Property GraficsDim  As Double = Double.NaN
            
            ''' <summary> Grafics Eccentricity. </summary>
            Public Property GraficsEcc  As Double = Double.NaN
            
            ''' <summary> Type of Coordinates, i.e. "YXZ". </summary>
            Public Property CoordType   As String = Nothing
            
            ''' <summary> Flags. </summary>
            Public Property Flags       As String = Nothing
            
            ''' <summary> Key of Attribute 1. </summary>
            Public Property AttKey1     As String = Nothing
            
            ''' <summary> Value of Attribute 1. </summary>
            Public Property AttValue1   As String = Nothing
            
            ''' <summary> Key of Attribute 2. </summary>
            Public Property AttKey2     As String = Nothing
            
            ''' <summary> Value of Attribute 2. </summary>
            Public Property AttValue2   As String = Nothing
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Parses the given string as new <see cref="GeoPoint.ID"/> for this GeoPoint. </summary>
             ''' <param name="TargetID"> The string which is intended to become the new point ID. It will be trimmed here. </param>
             ''' <returns> The parsed ID. </returns>
             ''' <remarks>
             ''' <para>
             ''' This method will be called when setting the <see cref="GeoPoint.ID"/> property.
             ''' It allows for special format conversions and for validation of <paramref name="TargetID"/>.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> <paramref name="TargetID"/> is longer than 20 characters. </exception>
            Protected Overrides Function ParseID(TargetID As String) As String
                
                Dim RetValue As String = MyBase.ParseID(TargetID)
                
                If (RetValue.Length > MaxIDLength) Then
                    Throw New InvalidIDException(sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_IDToLong, RetValue, MaxIDLength))
                End If
                
                Return RetValue
            End Function
            
            ''' <summary> Returns a very basic output of the point. </summary>
            Public Overrides Function ToString() As String
                Return sprintf(" %+20s %15.5f%15.5f%15.4f  %s", Me.ID, Me.Y, Me.X, Me.Z, Me.Info)
            End Function
            
        #End Region
        
        #Region "Public Methods"
            
            ''' <summary> A given String will be parsed for attributes and a comment. May be <see langword="null"/>. </summary>
             ''' <param name="FreeDataText"> The string to parse. </param>
             ''' <remarks>
             ''' <para>
             ''' <paramref name="FreeDataText"/> is required to look <b>like in ipkt, A0, A1</b>: <code>AttName|AttValue|AttName|AttValue|comment</code>. 
             ''' There may be zero, one or more attributes (name/value pairs). The attribute names are trimmed.
             ''' </para>
             ''' <para>
             ''' This method clears and sets the <see cref="GeoPoint.Attributes"/> and <see cref="GeoPoint.Comment"/> properties.
             ''' </para>
             ''' </remarks>
            Public Sub ParseFreeData(FreeDataText As String)
                
                Me.Attributes = Nothing
                Me.Comment    = String.Empty
                
                If (FreeDataText.IsNotEmptyOrWhiteSpace()) Then
                    
                    Dim FreeFields() As String = FreeDataText.Split(AttSeparator)
                    Dim i As Integer
                    For i = 0 To FreeFields.Length - 3 Step 2
                        If (i = 0) Then Me.Attributes = New Dictionary(Of String, String)
                        Me.Attributes.Add(FreeFields(i).Trim(), FreeFields(i + 1))
                    Next
                    Me.Comment = FreeFields(i)
                    If (FreeFields.Length > (i + 1)) Then
                        Me.Comment &= AttSeparator & FreeFields(i + 1)
                    End If
                End If
            End Sub
            
            Public Function GetFreeDataText() As String
                
                Dim FreeDataText As String = String.Empty
                                
                ' Attributes.
                If (Me.Attributes?.Count > 0) Then
                    Dim AttString As String = String.Empty
                    For Each kvp As KeyValuePair(Of String, String) In Me.Attributes
                        AttString &= kvp.Key & AttSeparator & kvp.Value & AttSeparator
                    Next
                    FreeDataText &= AttString
                End If
                
                ' Comment.
                If (Me.Comment.IsNotEmptyOrWhiteSpace()) Then
                    FreeDataText &= Me.Comment
                End If
                
                Return FreeDataText
            End Function
            
        #End Region

    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
