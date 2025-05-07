Feature: User Login
    As a registered user
    I want to be able to log in to the system
    So that I can access my account

Background:
    Given I am on the login page

Scenario: User sees the login form
    Then I should see the login form
    And I should see the email field
    And I should see the password field
    And I should see the login button
    And I should see the forgot password link
    And I should see the sign up link

Scenario: User submits empty form
    When I click the login button
    Then I should see validation errors for empty fields

Scenario: User enters invalid email format
    When I enter "not-an-email" in the email field
    And I enter "password123" in the password field
    And I click the login button
    Then I should see an email validation error

Scenario: User logs in successfully
    When I enter "test@example.com" in the email field
    And I enter "password123" in the password field
    And I click the login button
    Then I should be redirected to the home page

Scenario: User enters incorrect credentials
    When I enter "test@example.com" in the email field
    And I enter "wrong-password" in the password field
    And I click the login button
    Then I should see an error message "Invalid credentials"
    And I should see error details about incorrect credentials

Scenario: User wants to recover password
    When I click the forgot password link
    Then I should be redirected to the forgot password page

Scenario: User wants to create an account
    When I click the sign up link
    Then I should be redirected to the sign up page

Scenario: User sees loading indicator during login
    When I enter "test@example.com" in the email field
    And I enter "password123" in the password field
    And I click the login button
    Then I should see a loading indicator
    And the login button should be disabled