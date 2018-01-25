Imports System.Windows
Imports DevExpress.Xpf.Core
Imports DevExpress.Xpf.LayoutControl
Imports GameAdministratorCenter.Contracts

Public Class GameManagerUserControl
    Private selectedGame As GAME = Nothing

    Private Sub addButton_Click(sender As Object, e As RoutedEventArgs) Handles addButton.Click
        If tabControl.SelectedTabItem IsNot Nothing Then
            Dim gameType As GAME_TYPE = DirectCast(tabControl.SelectedTabItem.Tag, GAME_TYPE)
            tabControl.Visibility = Visibility.Collapsed
            addEditContainer.Visibility = Visibility.Visible
            addEditContainer.Content = New AddEditGameUserControl(Nothing, gameType)
        End If
    End Sub

    Private Sub backButton_Click(sender As Object, e As RoutedEventArgs) Handles backButton.Click
        tabControl.Visibility = Visibility.Visible
        addEditContainer.Visibility = Visibility.Collapsed
    End Sub

    Private Sub deleteButton_Click(sender As Object, e As RoutedEventArgs) Handles deleteButton.Click
        If selectedGame IsNot Nothing Then
            selectedGame.IS_DELETED = True
            getDatabaseEntity.SaveChanges()
            selectedGame = Nothing
            editButton.IsEnabled = False
            manageButton.IsEnabled = False
            deleteButton.IsEnabled = False
            populateTabs()
        End If
    End Sub

    Private Sub editButton_Click(sender As Object, e As RoutedEventArgs) Handles editButton.Click
        If tabControl.SelectedTabItem IsNot Nothing AndAlso selectedGame IsNot Nothing Then
            Dim gameType As GAME_TYPE = DirectCast(tabControl.SelectedTabItem.Tag, GAME_TYPE)
            tabControl.Visibility = Visibility.Collapsed
            addEditContainer.Visibility = Visibility.Visible
            addEditContainer.Content = New AddEditGameUserControl(selectedGame.ID, gameType)
        End If
    End Sub

    Private Sub GameManagerUserControl_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        populateTabs()
    End Sub

    Private Sub populateTabs()
        tabControl.Items.Clear()
        Dim gameTypeList As List(Of GAME_TYPE) = (From query In getDatabaseEntity.GAME_TYPE Where query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False).ToList
        If gameTypeList IsNot Nothing Then
            For Each gameType In gameTypeList
                Dim tab As New DXTabItem
                tab.Tag = gameType
                tab.Header = gameType.GAME_TYPE_NAME
                Dim flowLayout As New FlowLayoutControl
                flowLayout.Orientation = Controls.Orientation.Horizontal
                Dim gameList As List(Of GAME) = (From query In getDatabaseEntity.GAMEs Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso
                                                                                           query.GAME_TYPE_LINK_ID = gameType.ID).ToList
                If gameList IsNot Nothing Then
                    For Each game In gameList
                        Dim gameCardView = New GameCardView(game)
                        gameCardView.gameSelectedDelegate = AddressOf updateSelectedGame
                        flowLayout.Children.Add(gameCardView)
                    Next
                End If
                tab.Content = flowLayout
                tabControl.Items.Add(tab)
            Next
        End If
    End Sub

    Private Sub updateSelectedGame(game As GAME)
        selectedGame = game
        editButton.IsEnabled = True
        manageButton.IsEnabled = True
        deleteButton.IsEnabled = True
    End Sub

    Private Sub saveButton_Click(sender As Object, e As RoutedEventArgs) Handles saveButton.Click
        Try
            Dim addEditUserControl = DirectCast(addEditContainer.Content, AddEditGameUserControl)
            addEditUserControl.SaveChanges()
            populateTabs()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub manageButton_Click(sender As Object, e As RoutedEventArgs) Handles manageButton.Click
        If tabControl.SelectedTabItem IsNot Nothing AndAlso selectedGame IsNot Nothing Then
            tabControl.Visibility = Visibility.Collapsed
            addEditContainer.Visibility = Visibility.Visible
            addEditContainer.Content = New ManageGamePLayUserControl(selectedGame.ID)
        End If
    End Sub

    Private Sub createGameTypeButton_Click(sender As Object, e As RoutedEventArgs) Handles createGameTypeButton.Click
        tabControl.Visibility = Visibility.Collapsed
        addEditContainer.Visibility = Visibility.Visible
        addEditContainer.Content = New GameTypeCreationUserControl
    End Sub
End Class
