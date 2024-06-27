# Recipe App

## Overview
The Recipe App is a WPF application designed for managing and saving recipes. Users can add ingredients, steps, and save recipes to JSON files. This README provides an overview of how to use the app and its functionalities.

## Features
- **Add Recipe:** Allows users to enter recipe details including ingredients and steps.
- **Save Recipe:** Saves the entered recipe as a JSON file.
- **Validation:** Warns users if the total calories of ingredients exceed 300.

## Getting Started
To use the Recipe App:

1. **Clone the Repository:** Clone this repository to your local machine.

2. **Open the Project:** Open the project in Visual Studio (or your preferred IDE).

3. **Build and Run:** Build and run the project to launch the Recipe App.

## Usage
### Adding a Recipe
1. Launch the Recipe App.
2. Click on "Add Recipe" in the navigation panel.
3. Enter the recipe name in the provided textbox.
4. Add ingredients by clicking the "+" button to duplicate the ingredient entry fields.
5. Fill in the details for each ingredient (Name, Food Group, Amount, Unit, Calorie).
6. Add steps by clicking the "+" button to duplicate the step entry fields.
7. Fill in the description for each step.
8. Click "Save" to save the recipe as a JSON file. Select a location and name for the file.

### Additional Functionality
- **Reset:** Clears all fields and resets the form to add a new recipe.
- **Validation:** Displays a warning if the total calories of ingredients exceed 300.

## File Structure
- **MainWindow.xaml:** Main UI layout and navigation controls.
- **AddRecipe.xaml:** UI for adding a recipe, including dynamic ingredient and step entry.
- **ViewModels:** Contains view models used to bind data and commands to UI elements.
- **Utilities:** Custom controls and styles used in the application.

