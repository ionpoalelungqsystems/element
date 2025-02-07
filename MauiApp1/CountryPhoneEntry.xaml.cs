﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;
using CountryData.Standard;

namespace MauiApp1
{
    public partial class CountryPhoneEntry : ContentView
    {
        private ObservableCollection<Country> countries = new();
        private Country selectedCountry;
        private CountryHelper countryHelper; // Instance of CountryHelper

        public CountryPhoneEntry()
        {
            InitializeComponent();
            countryHelper = new CountryHelper(); // Initialize CountryHelper
            var countryData = countryHelper.GetCountryData().ToList();
            countries = new ObservableCollection<Country>(countryData.Select(c => new Country
            {
                Name = c.CountryName,
                CallingCode = "+" + c.PhoneCode.TrimStart('+'),
                Alpha2 = c.CountryShortCode.ToLower(),
                Flag = GetCountryEmojiFlag(c.CountryShortCode) // Use emoji flag instead of image
            }));
            BindingContext = this;
            selectedCountry = countries.FirstOrDefault(c => c.Alpha2 == "md") ?? countries.First();
            UpdateUI();
        }

        private string GetCountryEmojiFlag(string shortCode)
        {
            var flag = countryHelper.GetCountryEmojiFlag(shortCode); // Call the method using the instance
            Console.WriteLine($"Flag for {shortCode}: {flag}"); // Debugging information
            return flag;
        }

        private void OnCountryButtonClicked(object sender, EventArgs e)
        {
            CountryListBorder.IsVisible = !CountryListBorder.IsVisible;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue?.ToLower() ?? "";
            CountryListView.ItemsSource = string.IsNullOrEmpty(searchText)
                ? countries
                : countries.Where(c => c.Name.ToLower().Contains(searchText)).ToList();
        }

        private void OnCountrySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Country country)
            {
                selectedCountry = country;
                UpdateUI();
                CountryListBorder.IsVisible = false;
                CountryListView.SelectedItem = null;
            }
        }

        private void UpdateUI()
        {
            if (selectedCountry == null)
            {
                Console.WriteLine("Error: selectedCountry is null.");
                return;
            }

            if (SelectedFlag == null)
            {
                Console.WriteLine("Error: SelectedFlag is null.");
                return;
            }

            if (PhoneNumberEntry == null)
            {
                Console.WriteLine("Error: PhoneNumberEntry is null.");
                return;
            }

            SelectedFlag.Text = selectedCountry.Flag; // Display the emoji flag

            // Ensure only one plus sign is present in the phone number entry
            if (PhoneNumberEntry.Text == null || !PhoneNumberEntry.Text.StartsWith(selectedCountry.CallingCode))
            {
                PhoneNumberEntry.Text = selectedCountry.CallingCode;
            }
        }

        private void OnPhoneNumberTextChanged(object sender, TextChangedEventArgs e)
        {
            string phoneNumber = e.NewTextValue?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                ValidationMessage.IsVisible = false;
                return;
            }

            // Ensure the phone number starts with the calling code
            if (!phoneNumber.StartsWith(selectedCountry?.CallingCode))
            {
                PhoneNumberEntry.Text = selectedCountry?.CallingCode + phoneNumber.TrimStart('+');
                return;
            }

            // Remove all non-digit characters except for the calling code
            string digitsOnly = Regex.Replace(phoneNumber.Substring(selectedCountry.CallingCode.Length), @"[^\d]", "");
            int digitCount = digitsOnly.Length;

            // Validate phone number format
            Regex regex = new Regex(@"^\+\d{1,4} \d{6,12}$");
            if (!regex.IsMatch(phoneNumber))
            {
                ValidationMessage.Text = "Invalid phone number format.";
                ValidationMessage.TextColor = Colors.Red;
                ValidationMessage.IsVisible = true;
                return;
            }

            ValidationMessage.Text = $"Valid phone number! ({digitCount} digits)";
            ValidationMessage.TextColor = Colors.Green;
            ValidationMessage.IsVisible = true;
        }
    }

    public class Country
    {
        public required string Name { get; set; }
        public required string CallingCode { get; set; }
        public required string Alpha2 { get; set; }
        public required string Flag { get; set; } // New property for flag emoji
    }
}




