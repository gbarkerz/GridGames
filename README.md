# Grid Games

This puzzle app is a resources for developers to help them consider the accessibility of their own apps. The app particularly focuses on the experience for players who use screen readers. The source code for the app is publicly available at [Grid Games source code](https://github.com/gbarkerz/GridGames). The app is built using the MAUI UI technology, and its source code shows a number of accessibility-related topics described at [Build accessible apps with semantic properties](https://docs.microsoft.com/dotnet/maui/fundamentals/accessibility).

The Grid Games app contains two puzzle games, and both games can only be played only using the keyboard. Use the Tab key to move keyboard focus into the grid of squares shown in the games, then the Arrow keys to move around the grid, and then the Space or Enter keys to click a square.

The Pairs Game is based on a traditional card matching game, where face-down cards are turned over in order to find pairs of cards. When the game is run, a 4-by-4 grid of blank squares appears, with each square representing a face-down card. When one of the cards is turned up, it reveals an image on the face-up card. When another card is turned up, an image is also shown on that second face-up card. If the two images are the same, then the cards are considered to be matched, and will not change again for the rest of the game. If the images do not match, then the cards are considered to be unmatched, and cards must be then turned back down. These steps are then repeated until all matching images have been found, and a "Congratulations" window appears.

To have your own pictures shown in the Pairs game, along with custom accessible names and descriptions, please follow the steps described in the Pairs Settings page.

<img width="650" alt="The Pairs game in the Grid Games app showing eight pairs of matching upturned cards. The cards show outdoor scenes." src="https://user-images.githubusercontent.com/77085891/182926175-6608d180-92ae-4342-a588-e2de025a5423.png">

<img width="747" alt="The Pairs game in the Grid Games app running in dark mode, showing seven pairs of matching upturned cards, and one unmatched upturned card. The cards show indoor and outdoor scenes of a Tudor hall. The NVDA screen reader's Speech Viewer is showing the announcements made by NVDA as the Pairs game is played." src="https://user-images.githubusercontent.com/77085891/182926230-28da33fb-65c1-4cf4-bd28-877e888964bb.png">


The Where's WCAG? game aims to help players become more familiar with WCAG groupings. "WCAG" is the Web Content Accessibility Guidelines (WCAG) international standard, which helps web content authors create content that's more accessible. The WCAG standard has been used as the base for European accessibility standards which apply to both web content and software.

The Where's WCAG? game presents a question asking the player to find a particular WCAG group. For example, "Where's Enough Time?". Below the question in the app is a 4-by-4 grid of squares showing fifteen WCAG group numbers. For example, "2.2". The aim of the game is for the player to click the square in the grid which shows the number matching the WCAG group in the question. If successful, the square changes to also show both the number and name of the WCAG group, and the question changes to ask the player to find a different WCAG group. If not successful, the player tries again by clicking another square. Once all the WCAG groups are found, the player has won, and a "Congratulations" window appears. 

<img width="976" alt="The Where's WCAG? game in the Grid Games app showing the 4-by-4 grid of squares, with each square showing the number of a WCAG grouping. Nine of the squares also show the title of the WCAG whose number is shown on the square. The page has a heading of Where's Keyboard Accessible? and the square showing 2.1 is highlighted in the game. The NVDA screen reader's Speech Viewer is showing the announcements made by NVDA as the game is played." src="https://user-images.githubusercontent.com/77085891/182926429-4b08a12b-6fc8-4eac-a838-293a0c2ec643.png">

<img width="1128" alt="The Where's WCAG? game in the Grid Games app running in dark mode, showing the 4-by-4 grid of squares, with each square showing the number of a WCAG grouping. Seven of the squares also show the title of the WCAG whose number is shown on the square. The page has a heading of Where's Operable? and the square showing 2 is highlighted in the game." src="https://user-images.githubusercontent.com/77085891/182926463-2f03ef72-b3a5-4899-9c5d-ec9498b003f7.png">


Bonus WCAG-related questions can be presented while playing the Where's WCAG game. To have your own bonus questions asked in the Where's WCAG game, please follow the steps described in the Where's WCAG Settings page.

<img width="1128" alt="The Where's WCAG? game in the Grid Games app showing a bonus question. Four WCAG group pickers provide access to groups of WCAG, and the picker labelled Perceivable groups has 1.3 Adaptable selected. Groups of CheckBoxes relating to specific WCAG groups are shown, with on the CheckBox labelled 1.3.2 Meaningful Sequence selected. All interactable controls have their access keys shown." src="https://user-images.githubusercontent.com/77085891/182926532-778f2cac-e91e-4385-9418-9ddb39d145ee.png">

<img width="1047" alt="The Where's WCAG? game in the Grid Games app running in dark mode, showing a bonus question. Four WCAG group pickers provide access to groups of WCAG, and the picker labelled Understandable groups has 3.3 Input Assistance selected. Groups of CheckBoxes relating to specific WCAG groups are shown, with on the CheckBox labelled 3.3.2 Labels of Instructions selected." src="https://user-images.githubusercontent.com/77085891/182926558-683c98fd-b1d5-4ebc-b337-22a0950bcb43.png">


In either the Pairs or Where's WCAG? game, press the F1 key to show this Help content, or the F5 key to restart a game. Press and release the Alt key to have any available access keys shown. For example, press Alt+S to have the game's Settings page shown.
