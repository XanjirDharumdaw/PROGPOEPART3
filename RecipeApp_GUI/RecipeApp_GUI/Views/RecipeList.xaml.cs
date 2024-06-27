using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using RecipeApp_GUI.Utilities;

namespace RecipeApp_GUI.Views
{
    public partial class RecipeList : UserControl
    {
 
        public ObservableCollection<RecipeFile> recipeList { get; set; }
        public ObservableCollection<RecipeFile> filteredRecipeList { get; set; }
        private string searchText;
        //This well let the XAML bind the data points to this file
        public RecipeList()
        {
            InitializeComponent();
            recipeList = new ObservableCollection<RecipeFile>();
            filteredRecipeList = new ObservableCollection<RecipeFile>();
            DataContext = this;

            Loaded += RecipeList_Loaded;
        }

        private void RecipeList_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRecipeFiles();
        }

        private void LoadRecipeFiles()
        {
            // This is where our recipe folder path will be located.
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recipes");

            try
            {
                // This retrieves all the JSON files in the folder
                string[] filePaths = Directory.GetFiles(folderPath, "*.json");

                // Clear the existing recipe list
                recipeList.Clear();

                foreach (string filePath in filePaths)
                {
                    // This will read the contents of the file and deserialize the contents
                    string fileContents = File.ReadAllText(filePath);
                    Recipe recipe = JsonConvert.DeserializeObject<Recipe>(fileContents);

                    // This creates a new RecipeFile object and adds it to the collection, taking values from the file path
                    RecipeFile recipeFile = new RecipeFile
                    {
                        RecipeName = Path.GetFileNameWithoutExtension(filePath),
                        FilePath = filePath,
                        DateCreated = File.GetCreationTime(filePath),
                        DateModified = File.GetLastWriteTime(filePath),
                        Calorie = CalculateTotalCalories(filePath),
                        FoodGroup = GetFoodGroups(filePath)
                    };
                    recipeList.Add(recipeFile);
                }

                // This sorts the recipes in our DataGrid in alphabetical order.
                recipeList = new ObservableCollection<RecipeFile>(recipeList.OrderBy(r => r.RecipeName));
                filteredRecipeList = recipeList;
                recipeDataGrid.ItemsSource = filteredRecipeList; // Update the ItemsSource property

            }
            catch (Exception ex)
            {
                // This gives us an error message if the page can't find our folder or if the JSON information is wrong
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"An error occurred while loading recipe files: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
        }

        private int CalculateTotalCalories(string filePath)
        {
            try
            {
                string fileContents = File.ReadAllText(filePath);
                Recipe recipe = JsonConvert.DeserializeObject<Recipe>(fileContents);

                int totalCalories = 0;
                foreach (Ingredient ingredient in recipe.Ingredients)
                {
                    if (int.TryParse(ingredient.Calorie, out int calorie))
                    {
                        totalCalories += calorie;
                    }
                }
                return totalCalories;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while calculating total calories: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return 0;
            }
        }

        private string GetFoodGroups(string filePath)
        {
            try
            {
                string fileContents = File.ReadAllText(filePath);
                Recipe recipe = JsonConvert.DeserializeObject<Recipe>(fileContents);

                return string.Join(", ", recipe.Ingredients.Select(i => i.FoodGroup).Distinct());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while retrieving food groups: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return string.Empty;
            }
        }

        //This will display a message box for the recipe we selected as soon as we double click it.
        
        private void recipeDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RecipeFile selectedRecipe = recipeDataGrid.SelectedItem as RecipeFile;
            if (selectedRecipe != null)
            {
                string filePath = selectedRecipe.FilePath;

                try
                {
                    string fileContents = File.ReadAllText(filePath);
                    Recipe recipe = JsonConvert.DeserializeObject<Recipe>(fileContents);

                    RecipeDetailsWindow detailsWindow = new RecipeDetailsWindow(recipe); // Pass the Recipe object
                    detailsWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while reading the file: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }





        //This allows us to edit the contents of our JSON file 
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            RecipeFile selectedRecipe = recipeDataGrid.SelectedItem as RecipeFile;
            if (selectedRecipe != null)
            {
                // Access the file path
                string filePath = selectedRecipe.FilePath;

                try
                {
                    Process.Start("notepad.exe", filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while opening the recipe file: {ex.Message}\n\n{ex.StackTrace}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }


        
        //This button deletes the file in the folder
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            RecipeFile selectedRecipe = recipeDataGrid.SelectedItem as RecipeFile;
            if (selectedRecipe != null)
            {
                // Access the file path
                string filePath = selectedRecipe.FilePath;

                try
                {
                    // Delete the file
                    File.Delete(filePath);

                    // Remove the deleted recipe file from the collection
                    recipeList.Remove(selectedRecipe);
                    filteredRecipeList.Remove(selectedRecipe);

                    MessageBox.Show("File deleted successfully.", "File Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"An error occurred while loading recipe file '{filePath}': {ex}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    });
                }

            }
        }
        //This will allow the user to search for a recipe usin the name,calorie or food group
        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            searchText = searchTextBox.Text.ToLower();
            ApplyFilter();
        }
        //This method is our filter code that will implement the search filter logic for our datagrid.
        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredRecipeList = recipeList;
            }
            else
            {
                filteredRecipeList = new ObservableCollection<RecipeFile>(
                    recipeList.Where(r =>
                        r.RecipeName.ToLower().Contains(searchText) ||
                        r.Calorie.ToString().Contains(searchText) ||
                        (r.FoodGroup != null && r.FoodGroup.ToLower().Contains(searchText))
                    ));
            }

            recipeDataGrid.ItemsSource = filteredRecipeList;
        }
       

    }

    // this class holds the recipe information from our JSON which will be displayed in the datagrid.
    public class RecipeFile
    {
        public string RecipeName { get; set; }
        public string FilePath { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public int Calorie { get; set; }
        public string FoodGroup { get; set; }
    }

    // This recipe class is places our JSON file in the right structure 
    public class Recipe
    {
        public string Name { get; set; }
        public int Calorie { get; set; }
        public string FoodGroup { get; set; }
        public List<Ingredient> Ingredients { get; set; }
        public List<string> Instructions { get; set; }
    }

    // Define the Ingredient class to hold ingredient information
    public class Ingredient
    {
        public string Amount { get; set; }
        public string Unit { get; set; }
        public string IngName { get; set; }

        public string Calorie { get; set; }
        public string FoodGroup { get; set; }
    }
}
