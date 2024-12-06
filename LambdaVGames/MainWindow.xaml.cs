﻿using MySql.Data.MySqlClient;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace LambdaVGames;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    private readonly MySqlConnection connection;

    private List<Game> data = [];

    public MainWindow()
    {
        InitializeComponent();
        DatabaseDialog dbDialog = new();
        bool? result = dbDialog.ShowDialog();

        connection = MySqlInterop.Connection ?? throw new NullReferenceException("Database connection is null.");

        GamesBox.ItemsSource = data;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        connection.Close();

        base.OnClosing(e);
    }


    private void NameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {

    }

    private void CategoryTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {

    }

    private void DescriptionTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {

    }

    private void PriceTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {

    }


    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        NameTextBox.Text = data[GamesBox.SelectedIndex].name;
        DescriptionTextBox.Text = data[GamesBox.SelectedIndex].description;
    }
}