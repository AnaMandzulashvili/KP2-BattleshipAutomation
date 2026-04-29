Feature: Battleship Game

  As a player
  I want to play a full game of Battleship against a random opponent
  So that the outcome is correctly reported

  @SmokeTest
  @TestId_TC001
  Scenario: Player wins a full game against a random opponent
    Given The Battleship game page is loaded
    When My fleet is placed randomly
    Then The board is ready to play
    When I start a game against a random opponent
    And I play until the game ends
    Then I win the game
