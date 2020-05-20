﻿using Microsoft.DirectX.AudioVideoPlayback;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LinkaWPF
{
    /// <summary>
    /// Логика взаимодействия для EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {
        private IList<Models.Card> _cards;
        private CardButton _selectedCardButton;

        public EditorWindow()
        {
            InitializeComponent();

            _cards = new List<Models.Card>();

            cardBoard.Cards = _cards;
            cardBoard.ClickOnCardButton += ClickOnCardButton;
        }

        private void ClickOnCardButton(object sender, EventArgs e)
        {
            var cardButton = sender as CardButton;

            SelectCard(cardButton);
        }

        private void Play(object sender, RoutedEventArgs e)
        {
            if (_selectedCardButton == null || _selectedCardButton.Card == null || _selectedCardButton.Card.AudioPath == null || File.Exists(_selectedCardButton.Card.AudioPath) == false) return;

            var audio = new Audio(_selectedCardButton.Card.AudioPath);
            playButton.IsEnabled = false;
            audio.Ending += (s, args) => { playButton.IsEnabled = true; };
            audio.Play();
        }

        private void AddCard(object sender, RoutedEventArgs e)
        {
            var cardEditorWindow = new CardEditorWindow();
            cardEditorWindow.Owner = this;
            cardEditorWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (cardEditorWindow.ShowDialog() != true) return;

            var card = new Models.Card(_cards.Count, cardEditorWindow.Caption, cardEditorWindow.ImagePath, cardEditorWindow.AudioPath, cardEditorWindow.WithoutSpace);
            _cards.Add(card);

            cardBoard.Update(_cards);
        }

        private void EditCard(object sender, RoutedEventArgs e)
        {
            if (_selectedCardButton == null || _selectedCardButton.Card == null) return;

            var index = _cards.IndexOf(_selectedCardButton.Card);

            var cardEditorWindow = new CardEditorWindow(_selectedCardButton.Card.Title, _selectedCardButton.Card.WithoutSpace, _selectedCardButton.Card.ImagePath, _selectedCardButton.Card.AudioPath);
            cardEditorWindow.Owner = this;
            cardEditorWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (cardEditorWindow.ShowDialog() != true) return;

            _cards[index].Title = cardEditorWindow.Caption;
            _cards[index].WithoutSpace = cardEditorWindow.WithoutSpace;
            _cards[index].ImagePath = cardEditorWindow.ImagePath;
            _cards[index].AudioPath = cardEditorWindow.AudioPath;

            cardBoard.UpdateCard(index, _cards[index]);
        }

        private void RemoveCard(object sender, RoutedEventArgs e)
        {
            if (_selectedCardButton == null || _selectedCardButton.Card == null) return;

            _cards.Remove(_selectedCardButton.Card);
            RemoveSelectionCard();

            cardBoard.Update(_cards);
        }

        private void ChangeGridSize(object sender, RoutedEventArgs e)
        {
            var rows = Convert.ToInt32(rowsText.Text);
            var columns = Convert.ToInt32(columnsText.Text);

            if ( rows != cardBoard.Rows) cardBoard.Rows = rows;
            if (columns != cardBoard.Columns) cardBoard.Columns = columns;
        }

        private void SelectedCardChanged(Models.Card card)
        {
            playButton.IsEnabled = card == null || card.AudioPath == null || card.AudioPath == string.Empty ? false : true;
            editButton.IsEnabled = card == null ? false : true;
            deleteButton.IsEnabled = card == null ? false : true;
        }

        private void SelectCard(CardButton cardButton)
        {
            if (cardButton == null || cardButton.Card == null) return;

            RemoveSelectionCard();

            _selectedCardButton = cardButton;
            _selectedCardButton.Background = Brushes.Yellow;

            SelectedCardChanged(_selectedCardButton.Card);
        }

        private void RemoveSelectionCard()
        {
            if (_selectedCardButton == null) return;

            _selectedCardButton.Background = Brushes.White;
            _selectedCardButton = null;

            SelectedCardChanged(null);
        }

        private void PrevPage(object sender, RoutedEventArgs e)
        {
            if (cardBoard.PrevPage() == true) RemoveSelectionCard();
        }

        private void NextPage(object sender, RoutedEventArgs e)
        {
            if (cardBoard.NextPage() == true) RemoveSelectionCard();
        }
    }
}
