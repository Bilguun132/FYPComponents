Imports System.ComponentModel
Imports System.IO
Imports System.Threading
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Shapes
Imports GameAdministratorCenter.Contracts

Public Class ImageViewer
    Public Delegate Sub afterCloseFunction()
    Public afterCloseFunctionDelegate As afterCloseFunction = Nothing
    Public imageManager As CONTENT_MANAGER = Nothing
    Private loadThumbnail As Boolean = False
    Private useSQLQuery As Boolean = False
    Private origin As System.Windows.Point
    Private start As System.Windows.Point

    Private ctxTaskFactory As New TaskFactory
    Private currentlySelectedContentManagerId As Integer = 0
    Private allowManipulation As Boolean = True
    Private controlImage As Controls.Image
    Private imageStringId As Integer = -1
    Private isFullImage As Boolean = False

    Public Sub New(imageManagerId As Integer)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        imageManager = (From query In getDatabaseEntity().CONTENT_MANAGER Where query.ID = imageManagerId).FirstOrDefault
        If imageManager IsNot Nothing AndAlso imageManager.IMAGE_STRING IsNot Nothing Then
            Dim hexString As String = imageManager.IMAGE_STRING.IMAGE_HEX_STRING
            controlImage = convertHexStringToControlImage(hexString)
            If controlImage IsNot Nothing Then
                image.Source = controlImage.Source
            End If
        End If
    End Sub

    Public Sub New(inputString As String, Optional inputStringType As inputStringType = inputStringType.imageString)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Try
            If inputStringType = inputStringType.imageString Then
                controlImage = convertHexStringToControlImage(inputString)
                If controlImage IsNot Nothing Then
                    image.Source = controlImage.Source
                End If
            Else
                Dim drawingImage As System.Drawing.Image = System.Drawing.Image.FromFile(inputString)
                If drawingImage IsNot Nothing Then
                    controlImage = convertDrawingImageToControlImage(drawingImage)
                    image.Source = controlImage.Source
                End If
            End If
        Catch ex As Exception
            Dim textBlock As New TextBlock()
            textBlock.HorizontalAlignment = HorizontalAlignment.Center
            textBlock.VerticalAlignment = VerticalAlignment.Center
            textBlock.FontSize = 20
            textBlock.FontWeight = FontWeights.Bold
            border.Content = textBlock
        End Try
    End Sub

    Public Sub hideButtonPanel()
        buttonPanel.Visibility = Visibility.Collapsed
    End Sub

    Public Sub showButtonPanel()
        buttonPanel.Visibility = Visibility.Visible
    End Sub

    Public Sub hideCloseButton()
        closeButton.Visibility = Visibility.Collapsed
    End Sub

    Public Sub showCloseButton()
        closeButton.Visibility = Visibility.Visible
    End Sub

    Private Sub closeButton_Click(sender As Object, e As RoutedEventArgs) Handles closeButton.Click
        If afterCloseFunctionDelegate IsNot Nothing Then
            afterCloseFunctionDelegate()
        End If
    End Sub

    Private Sub border_MouseWheel(sender As Object, e As MouseWheelEventArgs) Handles border.MouseWheel
        If allowManipulation Then
            Dim p As Point = New Point()
            p.X = image.ActualWidth / 2
            p.Y = image.ActualHeight / 2

            Dim m As Matrix = image.RenderTransform.Value
            If (e.Delta > 0) Then
                m.ScaleAt(1.1, 1.1, p.X, p.Y)
            Else
                m.ScaleAt(1 / 1.1, 1 / 1.1, p.X, p.Y)
            End If

            image.RenderTransform = New MatrixTransform(m)
        End If
    End Sub

    Private Sub image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles image.MouseLeftButtonDown
        If allowManipulation Then
            image.CaptureMouse()
            start = e.GetPosition(Me)

            origin.X = image.RenderTransform.Value.OffsetX
            origin.Y = image.RenderTransform.Value.OffsetY
        End If
    End Sub

    Private Sub image_MouseMove(sender As Object, e As MouseEventArgs) Handles image.MouseMove
        If allowManipulation AndAlso image.IsMouseCaptured Then
            Dim p As System.Windows.Point = e.MouseDevice.GetPosition(Me)

            Dim m As Matrix = image.RenderTransform.Value
            m.OffsetX = origin.X + (p.X - start.X)
            m.OffsetY = origin.Y + (p.Y - start.Y)

            Dim mLayout As Matrix = image.LayoutTransform.Value
            mLayout.OffsetX = m.OffsetX
            mLayout.OffsetY = m.OffsetY

            image.RenderTransform = New MatrixTransform(m)
            image.LayoutTransform = New MatrixTransform(mLayout)
        End If
    End Sub

    Private Sub image_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs) Handles image.MouseLeftButtonUp
        If allowManipulation Then
            image.ReleaseMouseCapture()
        End If
    End Sub

    Private Sub border_RequestBringIntoView(sender As Object, e As RequestBringIntoViewEventArgs) Handles border.RequestBringIntoView
        e.Handled = True
    End Sub

    Public Sub enableImageManipulation()
        allowManipulation = True
    End Sub

    Public Sub disableImageManipulation()
        allowManipulation = False
    End Sub

    Private Sub originalSizeButton_Click(sender As Object, e As RoutedEventArgs) Handles originalSizeButton.Click
        renderOrignalSize()
    End Sub

    Private Sub fullScreenSizeButton_Click(sender As Object, e As RoutedEventArgs) Handles fullScreenSizeButton.Click
        fitToContainerSize()
    End Sub

    Private Sub image_ManipulationDelta(sender As Object, e As ManipulationDeltaEventArgs) Handles image.ManipulationDelta
        Dim rectToMove As UIElement = TryCast(e.OriginalSource, UIElement)
        Dim rectsMatrix As Matrix = DirectCast(rectToMove.RenderTransform, MatrixTransform).Matrix

        rectsMatrix.RotateAt(e.DeltaManipulation.Rotation, e.ManipulationOrigin.X, e.ManipulationOrigin.Y)

        rectsMatrix.ScaleAt(e.DeltaManipulation.Scale.X, e.DeltaManipulation.Scale.X, e.ManipulationOrigin.X, e.ManipulationOrigin.Y)

        rectsMatrix.Translate(e.DeltaManipulation.Translation.X, e.DeltaManipulation.Translation.Y)

        rectToMove.RenderTransform = New MatrixTransform(rectsMatrix)
        Dim containingRect As New Rect(DirectCast(e.ManipulationContainer, FrameworkElement).RenderSize)

        Dim shapeBounds As Rect = rectToMove.RenderTransform.TransformBounds(New Rect(rectToMove.RenderSize))

        If e.IsInertial AndAlso Not containingRect.Contains(shapeBounds) Then
            e.Complete()
        End If

        e.Handled = True
    End Sub

    Private Sub image_ManipulationStarting(sender As Object, e As ManipulationStartingEventArgs) Handles image.ManipulationStarting
        e.ManipulationContainer = Me
        e.Handled = True
    End Sub

    Private Sub image_ManipulationInertiaStarting(sender As Object, e As ManipulationInertiaStartingEventArgs) Handles image.ManipulationInertiaStarting
        e.TranslationBehavior.DesiredDeceleration = 10.0 * 96.0 / (1000.0 * 1000.0)
        e.ExpansionBehavior.DesiredDeceleration = 0.1 * 96 / (1000.0 * 1000.0)
        e.RotationBehavior.DesiredDeceleration = 720 / (1000.0 * 1000.0)
        e.Handled = True
    End Sub

    Private Sub mainContainer_RequestBringIntoView(sender As Object, e As RequestBringIntoViewEventArgs) Handles mainContainer.RequestBringIntoView
        e.Handled = True
    End Sub

    Public Enum inputStringType
        fileName = 1
        imageString = 2
    End Enum

    Public Sub fitToContainerSize()
        image.Source = Nothing
        image.RenderTransform = New MatrixTransform()
        image.Source = controlImage.Source
    End Sub

    Public Sub renderOrignalSize()
        image.Source = Nothing
        image.RenderTransform = New MatrixTransform()
        image.Source = controlImage.Source
        Dim p As Point = New Point()
        p.X = image.ActualWidth / 2
        p.Y = image.ActualHeight / 2

        Dim m As Matrix = image.RenderTransform.Value
        Dim transformScale = image.Source.Height / image.ActualHeight
        m.ScaleAt(transformScale, transformScale, p.X, p.Y)

        image.RenderTransform = New MatrixTransform(m)
    End Sub
End Class
