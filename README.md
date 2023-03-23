# Grid Games

The app is a resource for developers to help them consider the accessibility of their own apps. For more technical details on the game, please visit the following articles: [Considerations Around Building an Accessible Multi-platform App](https://www.linkedin.com/pulse/considerations-around-building-accessible-app-guy-barker), [Using Visual Studio’s integrated Accessibility Checker with a new .NET MAUI puzzle game](https://www.linkedin.com/pulse/using-visual-studios-integrated-accessibility-checker-guy-barker), and [Can Sudoku be accessible?](https://www.linkedin.com/pulse/can-sudoku-accessible-guy-barker) The app is built using the .NET MAUI technology, and its source code shows a number of accessibility-related topics described at [Build accessible apps with semantic properties](https://docs.microsoft.com/dotnet/maui/fundamentals/accessibility).

**Please note that the code is only being made available here in order to share examples of accessibilty-related actions. No other best practices are shown, and the app code is in need of some serious code clean-up.**

When playing the games with the keyboard, use the Tab key to move keyboard focus into the grid of squares shown in the games, then the Arrow keys to move around the grid, and then the Space or Enter keys to click a square. In any of the games, press the F1 key to show this Help content, or the F5 key to restart a game. Press and release the Alt key to have any available access keys shown. For example, press Alt+S to have the game's Settings page shown.

The Pairs Game in the app is based on a traditional card matching game, where face-down cards are turned over in order to find pairs of cards. When the game is run, a 4-by-4 grid of blank squares appears, with each square representing a face-down card. When one of the cards is turned up, it reveals an image on the face-up card. When another card is turned up, an image is also shown on that second face-up card. If the two images are the same, then the cards are considered to be matched, and will not change again for the rest of the game. If the images do not match, then the cards are considered to be unmatched, and cards must be then turned back down. These steps are then repeated until all matching images have been found, and a "Congratulations" window appears.

To have your own pictures shown in the Pairs game, along with custom accessible names and descriptions, please follow the steps described in the Pairs Settings page.

<img width="650" alt="The Pairs game in the Grid Games app showing eight pairs of matching upturned cards. The cards show outdoor scenes." src="https://user-images.githubusercontent.com/77085891/182926175-6608d180-92ae-4342-a588-e2de025a5423.png">

<img width="747" alt="The Pairs game in the Grid Games app running in dark mode, showing seven pairs of matching upturned cards, and one unmatched upturned card. The cards show indoor and outdoor scenes of a Tudor hall. The NVDA screen reader's Speech Viewer is showing the announcements made by NVDA as the Pairs game is played." src="https://user-images.githubusercontent.com/77085891/182926230-28da33fb-65c1-4cf4-bd28-877e888964bb.png">

The other three games in the app are Sudoku, Leaf Sweeper, and Squares. For details on how to play these games, please visit the help content contained in the apps.

