# 1. Project Goal
The goal of the project is to develop a donation system for authors (hereinafter referred to as the System). Authors will be able to create different types of subscriptions in the System, manage their costs, and publish content for different subscription levels.
In the future, the System can be expanded to integrate with any social network that provides the ability to add members via a link and has mechanisms to track unauthorized access (i.e., it allows verifying that a user who has accessed the link is indeed a subscriber).
# 2. System Description
The System consists of the following main functional blocks:
1. Registration, authentication, and authorization
2. Functionality for authors
3. Subscription payment or one-time payment functionality and the main page
4. Functionality for subscribers
5. Notifications about new posts

## 2.1. User Types
The System includes two types of users: authors and subscribers. Authors create subscriptions and publish content, while subscribers, based on their subscription level, gain access to permitted content.
## 2.2. Registration
The System, at least in its first version, is not intended as a SaaS for multiple authors. A single installation of the System is tied to one specific author/project/channel. However, due to the open-source nature, anyone can download and deploy the System.
Thus, managing multiple channels/authors within a single installation of the System is not currently envisioned.
The process of creating an author in the System installation can be implemented in the first version not through the System interface but via a server command, which is acceptable. This command for creating an author should accept the following input data:
- Email — mandatory field
- Password — mandatory field

The process of creating an author via a server command should be documented in the System's documentation.
The registration process for subscribers, of course, must be implemented in the System interfaces. During subscriber registration, the following fields should be requested:
- Email — mandatory field
- First and last name — mandatory field
- Phone number -optional field

After submitting the registration form, the subscriber receives an email with the first code to log in. Authentication will be done using one-time codes sent to the email.
## 2.3. Authentication for Authors and Subscribers
Authentication for authors and subscribers is done via email and a one-time password (or a one-time link) sent to that email.
## 2.4. Functionality for Authors
After authentication (entering login and password), the author gains access to their author functionality in the System. This functionality consists of the following blocks:
1. Editing profile data
2. Creating and editing subscription types
3. Creating and editing content
4. Creating and editing goals
5. Email campaigns
6. Analytics

### 2.4.1. Editing Profile
In this section, the author can edit their profile data — email, project name, project description, social networks. Possible social networks:
- YouTube
- Instagram
- Facebook
- Linkedin
- Twitch

There should be an option to change the password by confirming the old password.
If the System design implies any images for customizing the System page, these images should also be editable from the author's profile.
### 2.4.2. Creating and Editing Subscription Types
For each subscription type, the following is set:
- Subscription name
- Subscription description
- **Possibly — a subscription cover image for use in the design of purchasing a particular subscription**
- Monthly subscription cost in $

There should be an option (optional) to set discounts for purchasing a subscription for 3, 6, and 12 months. The discount is set in $ for the purchased number of months.
### 2.4.3. Creating and Editing Content
Content published by the author can be formatted text with at least the ability to bold, italicize, underline, and strikethrough text. It might also be worth adding the ability to create headings of different sizes to structure long text content.
There should be an option to add lists and images to the post content, as well as attach files.
Each post should have a title.
There should be an option to create a post teaser — a cover image and a description of what the post contains. This teaser will be visible to guests (users who have not authenticated in the System) and those whose subscription level does not allow them to view the post.
Additionally, each post should have a subscription level assigned, indicating when the post can be read. Besides paid subscription levels, there should be a subscription level "open to all" — such posts will be visible to all System users, including unauthenticated guests.
There should also be an option to set a separate price for a post. That is, some posts can only be purchased individually for a one-time payment or can be both purchased individually and available as part of one of the subscriptions.
The parts of a post are the content itself and the teaser. The teaser is what is visible on the main page to both those who have access to the post and those who do not. Clicking on the teaser opens the post for those who have access to it.
The content can include: formatted text (headings, bold, italic, strikethrough text), images (uploaded from a computer), YouTube videos (displayed in the standard embedded YouTube player), audio (uploaded from a computer, played in the System's built-in audio player, only MP3 format is supported). There should also be an option to attach an arbitrary file uploaded from a computer — for subscribers, it will be displayed as a link to download the file.
### 2.4.4. Creating and Editing Goals
The author can raise funds for a specific goal within the System. For this, they create (and can later edit) a goal, within which the following is set:
- Goal description — a short text describing what the funds are being raised for
- Target amount in $

On the public page, the goal is displayed with its description and current progress, for example, "107 out of 130$ collected."
There can be multiple goals. Payment for a goal is a one-time payment, not recurring, meaning it is not deducted monthly.
### 2.4.5. Email Campaigns
The author can send email campaigns from the System. The email text is set, using an editor similar to the one used for creating posts, and the recipients are selected — free subscribers or subscribers of a certain level or all. Multiple options can be selected, for example, free subscribers and subscribers of a specific level.
### 2.4.6. Analytics
The author sees the following analytics:
1. Absolute and relative numbers of subscribers at different subscription levels. Possibly displayed as a pie chart.
2. Payments — listed, plus an export to Excel of all payments. Columns: payment date-time, payment amount, period (for subscriptions, not one-time payments), subscriber — id (a long 32-character hash, can be shown on mouse hover), name, email.

## 2.5. Subscription Payment or One-Time Payment Functionality and Main Page
The main page of the System is a list of the author's posts. If a guest views it, they only see the post teasers and posts open to all. On this page, the available subscription options are also visible — with the name of each subscription, its description, and price. Next to each subscription type is a Subscribe button, leading to the login or registration interface for the subscriber and subsequent payment.
Next to each teaser for a hidden post, the required subscription level to view the post should be displayed — or the option to purchase this individual post for its set price.
Post pagination is done via a "Show more" button at the bottom of the displayed posts. Clicking this button loads and displays older posts. New posts are shown at the top, older ones at the bottom.
There should also be an option to like posts — available only to subscribers. The number of likes is visible to everyone.
There should be an option to subscribe without payment. Such users will receive updates via email.
When paying for a subscription, there should be an option to pay immediately for 3, 6, or 12 months (the author can set discounts for such payments).
The total number of subscribers (both paid and free) should be displayed somewhere on the main page of the System.
Each post should have its own permanent link that can be saved. The post ID embedded in the link should be a UUID, not an integer, to prevent guessing the number of posts in the system.
### 2.5.1. Recurring Payments
Recurring payments should be deducted 30 days after the previous payment. If the payment cannot be deducted — within the next 5 days, daily attempts should be made to deduct the amount. During these 5 days, access is retained, and the subscriber receives an email after each unsuccessful attempt. After a successful deduction, the subscriber also receives an email.
After 5 days of unsuccessful attempts, the subscription is stopped, and the subscriber loses access to posts.
Email templates are not editable in the System interfaces; they are hardcoded in the System and can only be edited by modifying these templates in the System's code.
## 2.6. Functionality for Subscribers
Subscribers can view posts and like them, as well as upgrade their subscription level and purchase individual posts not included in their subscription.
Comments under posts are not envisioned. Post sharing is not envisioned as a separate button — those who want to share can do so by posting the link. Only favorite posts can be shown separately.
Each post can be added to favorites. In the favorites section, the subscriber should be able to create thematic lists. When adding a post to favorites, checkboxes for these lists should be shown, allowing the subscriber to immediately place the post in one of them.
Additionally, the subscriber should be able to edit their data (email, change password) on their profile page and cancel their subscription, which will stop the recurring monthly payments, and after the end of the paid period, the subscriber becomes a free subscriber.
The profile should have an option to disable email notifications from the system.
## 2.7. Notifications About New Posts
Subscribers should automatically receive email notifications about all new posts by the author in the System. Subscribers can opt out of these and other emails in their profile settings.
# 3. Proposed Technology Stack

### **NOT FINAL**

The following technology stack is proposed for implementing the system:
- Backend:
    - C#
    - PostgreSQL

- Frontend:
    - React
    - TypeScript

For internet acquiring, Stripe is being considered. During the project, the technical capabilities of platforms and payment fees should be compared. It is important to have the ability to pay with Google Pay and Apple Pay, including recurring payments (if Google Pay and Apple Pay allow this, clarify during the project).
File and image storage, uploaded by the author, should be done in an S3-compatible storage.
# 4. Design Requirements
Minimalism, conciseness, focus on content. White background. The System logo should be present somewhere on the page. The logo should be developed as part of this project.
At the bottom of the page (in the footer), it should say:
"Powered by Open Source" with a link to the project's GitHub.
