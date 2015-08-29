﻿
Imports System
Imports System.Math

Imports Rstyx.Utilities.Domain
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.StringUtils

Namespace Domain
    
    ''' <summary> One single VE point. </summary>
    ''' <remarks>
    ''' The <see cref="GeoPoint.ID"/> is maintained as String representation of an integer with a configurable maximum length.
    ''' </remarks>
    Public Class GeoVEPoint
        Inherits GeoPoint
        
        #Region "ID Definition Fields"
            
            ''' <summary> Gets the default maximum length of <see cref="GeoVEPoint.ID"/>. </summary>
            Public Shared ReadOnly DefaultMaxIDLength As Integer = 7
            
            ''' <summary> Gets the maximum length of <see cref="GeoVEPoint.ID"/> as determined at instantiation. </summary>
            Public ReadOnly MaxIDLength   As Integer
            
            ''' <summary> Gets the factor that converts a double <see cref="GeoVEPoint.ID"/> into an integer one (derived from <see cref="GeoVEPoint.MaxIDLength"/>). </summary>
            Public ReadOnly PointNoFactor As Integer
            
            ''' <summary> Gets the minimal integer <see cref="GeoVEPoint.ID"/> (always <b>1</b>). </summary>
            Public ReadOnly MinIntegerID  As Integer
            
            ''' <summary> Gets the maximum integer <see cref="GeoVEPoint.ID"/> (derived from <see cref="GeoVEPoint.MaxIDLength"/>). </summary>
            Public ReadOnly MaxIntegerID  As Integer
            
        #End Region
        
        #Region "Constuctor"
            
            ''' <summary> Creates a new, empty <see cref="GeoVEPoint"/> with a <see cref="GeoVEPoint.MaxIDLength"/> of <see cref="GeoVEPoint.DefaultMaxIDLength"/>. </summary>
            Public Sub New()
                Me.MaxIDLength   = DefaultMaxIDLength
                Me.PointNoFactor = Pow(10, MaxIDLength - 2)
                Me.MinIntegerID  = 1
                Me.MaxIntegerID  = Pow(10, MaxIDLength) - 1
            End Sub
            
            ''' <summary> Creates a new, empty <see cref="GeoVEPoint"/> with a given Value for <see cref="GeoVEPoint.MaxIDLength"/>. </summary>
             ''' <param name="MaxIDLength"> The maximum supported length resp. digits for the point ID. </param>
             ''' <remarks>  </remarks>
             ''' <exception cref="System.ArgumentOutOfRangeException"> <paramref name="MaxIDLength"/> doesn't equals 6 or 7. </exception>
            Public Sub New(MaxIDLength As Integer)
                
                If ((MaxIDLength < 6) OrElse (MaxIDLength > 7)) Then Throw New ArgumentOutOfRangeException("MaxIDLength")
                
                Me.MaxIDLength   = MaxIDLength
                Me.PointNoFactor = Pow(10, MaxIDLength - 2)
                Me.MinIntegerID  = 1
                Me.MaxIntegerID  = Pow(10, MaxIDLength) - 1
            End Sub
            
            ''' <summary> Creates a new <see cref="GeoVEPoint"/> with a <see cref="GeoVEPoint.MaxIDLength"/> of
            ''' <see cref="GeoVEPoint.DefaultMaxIDLength"/> and inititializes it's properties from a given <see cref="IGeoPoint"/>.
            ''' </summary>
             ''' <param name="SourcePoint"> The source point to get init values from. May be <see langword="null"/>. </param>
             ''' <remarks></remarks>
             ''' <exception cref="InvalidIDException"> ID of <paramref name="SourcePoint"/> isn't a valid ID for this point. </exception>
            Public Sub New(SourcePoint As IGeoPoint)
                
                Me.New()
                Me.GetPropsFromIGeoPoint(SourcePoint)
                
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
                
        #Region "Instance Methods"
            
            ''' <summary> Gets a Double for VE flavoured storage from this point's ID. </summary>
             ''' <returns> Me.ID / PointNoFactor or 1.0E+40 if ID is still <see langword="null"/>. </returns>
            Public Function IDToVEDouble() As Double
                Return If(Me.ID.IsEmptyOrWhiteSpace(), 1.0E+40, CDbl(Me.ID) / PointNoFactor)
            End Function
            
            ''' <summary> Gets a Single for VE flavoured storage from this point's ID. </summary>
             ''' <returns> Me.ID / PointNoFactor or Zero if ID is still <see langword="null"/>. </returns>
            Public Function IDToVESingle() As Single
                Return If(Me.ID.IsEmptyOrWhiteSpace(), 0.0, CSng(Me.ID) / PointNoFactor)
            End Function
            
            ''' <summary> Gets an Int32 for VE flavoured storage from this point's ID. </summary>
             ''' <returns> Me.ID or Zero if ID is still <see langword="null"/>. </returns>
            Public Function IDToVEInt32() As Int32
                Return If(Me.ID.IsEmptyOrWhiteSpace(), 0, CInt(Me.ID))
            End Function
            
            ''' <summary> Validates a given integer ID. </summary>
             ''' <param name="IntegerID"> The ID to validate </param>
             ''' <returns> <see langword="true"/>, if <paramref name="IntegerID"/> could be applied as <see cref="GeoVEPoint.ID"/>. </returns>
            Public Function IsValidID(IntegerID As Integer) As Boolean
                Return ((Me.MinIntegerID <= IntegerID) AndAlso (IntegerID <= Me.MaxIntegerID))
            End Function
            
            ''' <summary> Validates a given double ID. </summary>
             ''' <param name="DoubleID"> The ID to validate </param>
             ''' <returns> <see langword="true"/>, if <paramref name="DoubleID"/> could be applied as <see cref="GeoVEPoint.ID"/>. </returns>
            Public Function IsValidID(DoubleID As Double) As Boolean
                
                Dim MinDoubleID As Double = Me.MinIntegerID / Me.PointNoFactor
                Dim MaxDoubleID As Double = Me.MaxIntegerID / Me.PointNoFactor
                Dim ModDoubleID As Double = Round(DoubleID, Me.MaxIDLength)
                
                Return ((MinDoubleID <= ModDoubleID) AndAlso (ModDoubleID <= MaxDoubleID))
            End Function
            
            ''' <summary> Validates a given single ID. </summary>
             ''' <param name="SingleID"> The ID to validate </param>
             ''' <returns> <see langword="true"/>, if <paramref name="SingleID"/> could be applied as <see cref="GeoVEPoint.ID"/>. </returns>
            Public Function IsValidID(SingleID As Single) As Boolean
                Return IsValidID(CDbl(SingleID))
            End Function
            
            
            ''' <summary> Gets a decimal string representation from this point's ID. </summary>
             ''' <returns> I.e. "12.34567", "1.23000" , or an empty string if ID is still <see langword="null"/>. </returns>
            Public Function FormatID() As String
                Return If(Me.ID.IsEmptyOrWhiteSpace(), String.Empty, sprintf("%." & CStr(MaxIDLength - 2) & "f", CDbl(Me.ID) / PointNoFactor))
            End Function
            
        #End Region
        
        #Region "Static Methods"
            
            ''' <summary> Gets the usual (decimal) VE notation from a given ID. </summary>
             ''' <param name="ID"> The point ID to format. May be <see langword="null"/>. </param>
             ''' <returns> I.e. "12.34567", "1.23000" , or an empty string if ID is <see langword="null"/>. </returns>
             ''' <exception cref="InvalidIDException"> <paramref name="ID"/> isn't a valid ID for this point. </exception>
            Public Shared Function FormatID(ID As String) As String
                Dim RetID As String = ID
                If (ID.IsNotEmptyOrWhiteSpace()) Then
                    Dim VEPoint As New GeoVEPoint()
                    VEPoint.ID = ID
                    RetID = VEPoint.FormatID()
                End If
                Return RetID
            End Function
            
            ''' <summary> Gets the usual (decimal) VE notation from a given ID. </summary>
             ''' <param name="ID"> The point ID to format. May be <see langword="null"/>. </param>
             ''' <param name="MaxIDLength"> The maximum supported length resp. digits for the point ID. </param>
             ''' <returns> I.e. "12.34567", "1.23000" , or an empty string if ID is <see langword="null"/>. </returns>
             ''' <exception cref="System.ArgumentOutOfRangeException"> <paramref name="MaxIDLength"/> doesn't equals 6 or 7. </exception>
             ''' <exception cref="InvalidIDException"> <paramref name="ID"/> isn't a valid ID for this point. </exception>
            Public Shared Function FormatID(ID As String, MaxIDLength As Integer) As String
                Dim RetID As String = ID
                If (ID.IsNotEmptyOrWhiteSpace()) Then
                    Dim VEPoint As New GeoVEPoint(MaxIDLength)
                    VEPoint.ID = ID
                    RetID = VEPoint.FormatID()
                End If
                Return RetID
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
                    Message = sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_IDNotNumeric, RetValue)
                Else
                    Dim MinDoubleID As Double  = MinIntegerID
                    Dim MaxDoubleID As Double  = MaxIntegerID
                    If (RetValue.Contains(".") OrElse ((DoubleID > 0.0) AndAlso (DoubleID < 1.0))) Then DoubleID = DoubleID * PointNoFactor
                    DoubleID = Round(DoubleID, 0)
                    
                    If ((DoubleID < MinDoubleID) OrElse (DoubleID > MaxDoubleID)) Then
                        success = False
                        Message = sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_IDOutOfIntRange, RetValue, MinIntegerID, MaxIntegerID)
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
