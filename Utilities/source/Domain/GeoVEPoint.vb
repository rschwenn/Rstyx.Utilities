﻿
Imports System.Math

Imports Rstyx.Utilities.Domain
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain
    
    ''' <summary> One single VE point. </summary>
    ''' <remarks>
    ''' The <see cref="GeoPoint.ID"/> is maintained as String representation of an integer with maximum length of 7 digits.
    ''' </remarks>
    Public Class GeoVEPoint
        Inherits GeoPoint
        
        #Region "Private Fields"
            
            Private Shared MaxIDLength     As Integer = 7
            Private Shared PointNoFactor   As Integer = Pow(10, MaxIDLength - 2)
            Private Shared MinIntegerID    As Integer = 1
            Private Shared MaxIntegerID    As Integer = Pow(10, MaxIDLength) - 1
            
        #End Region
        
        #Region "Constuctor"
            
            ''' <summary> Creates a new, empty <see cref="GeoVEPoint"/>. </summary>
            Public Sub New()
            End Sub
            
            ''' <summary> Creates a new <see cref="GeoVEPoint"/> and inititializes it's properties from any given <see cref="IGeoPoint"/>. </summary>
             ''' <param name="SourcePoint"> The source point to get init values from. May be <see langword="null"/>. </param>
             ''' <remarks></remarks>
             ''' <exception cref="InvalidIDException"> ID of <paramref name="SourcePoint"/> isn't a valid ID for this point. </exception>
            Public Sub New(SourcePoint As IGeoPoint)
                MyBase.New(SourcePoint)
                
                ' KV + TC specials.
                If (TypeOf SourcePoint Is GeoVEPoint) Then
                    
                    Dim SourceVEPoint As GeoVEPoint = DirectCast(SourcePoint, GeoVEPoint)
                    
                    Me.HeightPostInfo   = SourceVEPoint.HeightPostInfo
                    Me.HeightPreInfo    = SourceVEPoint.HeightPreInfo
                    Me.PositionPostInfo = SourceVEPoint.PositionPostInfo
                    Me.PositionPreInfo  = SourceVEPoint.PositionPreInfo
                    Me.TrackPos         = SourceVEPoint.TrackPos
                    
                ElseIf (TypeOf SourcePoint Is GeoTcPoint) Then
                    
                    Dim SourceTcPoint As GeoTcPoint = DirectCast(SourcePoint, GeoTcPoint)
                    
                    Me.TrackPos.Kilometer = SourceTcPoint.Km
                End If
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            ''' <summary> General position description related to a railway track. </summary>
            Public Property TrackPos()          As PositionAtTrack = New PositionAtTrack()
            
            ''' <summary> Pre character of position info. </summary>
            Public Property PositionPreInfo     As Char = " "c
            
            ''' <summary> Post character of position info. </summary>
            Public Property PositionPostInfo    As Char = " "c
            
            ''' <summary> Pre character of height info. </summary>
            Public Property HeightPreInfo       As Char = " "c
            
            ''' <summary> Post character of height info. </summary>
            Public Property HeightPostInfo      As Char = " "c
            
        #End Region
                
        #Region "Methods"
            
            ''' <summary> Gets a Double for VE flavoured storage from this point's ID. </summary>
             ''' <returns> Me.ID / PointNoFactor or 1.0E+40 if ID is still <see langword="null"/>. </returns>
            Public Function IDToVEDouble() As Double
                Return IIf(Me.ID.IsEmptyOrWhiteSpace(), 1.0E+40, CDbl(Me.ID) / PointNoFactor)
            End Function
            
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
             ''' <para>
             ''' There are <b>two supported formats</b> for the point ID:
             ''' <list type="bullet">
             ''' <item><description> Always Positive numeric </description></item>
             ''' <item><description> If ID contains a ".", then valid range is: 0.00001 - 99.99999 </description></item>
             ''' <item><description> If ID doesn't contain a ".", then valid range is: 1 - 9999999 </description></item>
             ''' <item><description>  </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> <paramref name="TargetID"/> isn't a valid ID for this point - see above. </exception>
            Protected Overrides Function ParseID(TargetID As String) As String
                
                Dim RetValue As String  = MyBase.ParseID(TargetID)
                Dim DoubleID As Double
                Dim Message  As String  = String.Empty
                Dim success  As Boolean = Double.TryParse(RetValue, DoubleID)    
                
                ' Verify given ID.
                If ((Not success) OrElse Double.IsNaN(DoubleID)) Then
                    success = False
                    Message = sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_PointIDNotNumeric, RetValue)
                Else
                    Dim MinDoubleID As Double  = MinIntegerID
                    Dim MaxDoubleID As Double  = MaxIntegerID
                    If (RetValue.Contains(".")) Then DoubleID = DoubleID * PointNoFactor
                    DoubleID = Round(DoubleID, 0)
                    
                    If ((DoubleID < MinDoubleID) OrElse (DoubleID > MaxDoubleID)) Then
                        success = False
                        Message = sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_PointIDOutOfIntRange, RetValue, MinIntegerID, MaxIntegerID)
                    End If
                End If
                If (Not success) Then Throw New InvalidIDException(Message)
                
                ' Format validated ID.
                RetValue = sprintf("%.0f", DoubleID)
                
                Return RetValue
            End Function
            
            ''' <summary> Returns a KV output of the point. </summary>
            Public Overrides Function ToString() As String
                
                Dim KvFmt As String = "%+7s %15.5f%15.5f%10.4f %12.4f  %-13s %-13s %-4s %4d %1s  %3s %5.0f %5.0f  %1s %+3s  %1s%1s  %-8s %7s"
                
                Return sprintf(KvFmt, Me.ID, Me.Y, Me.X, Me.Z, Me.TrackPos.Kilometer.Value, Me.Info.TrimToMaxLength(13), Me.HeightInfo.TrimToMaxLength(13),
                               Me.Kind.TrimToMaxLength(4), Me.TrackPos.TrackNo, Me.TrackPos.RailsCode.TrimToMaxLength(1), Me.HeightSys.TrimToMaxLength(3), Me.mp, Me.mh, 
                               Me.MarkHints.TrimToMaxLength(1), Me.MarkType.TrimToMaxLength(3), Me.sp.TrimToMaxLength(1), Me.sh.TrimToMaxLength(1),
                               Me.Job.TrimToMaxLength(8), Me.ObjectKey.TrimToMaxLength(7))
            End Function
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4: