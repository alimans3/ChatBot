# Sales Bot
Sales bot built with Microsoft Bot Framework v4 for customers to order products based on their profiles in Microsoft Dynamics 365 Business Central.

This bot will chat with a customer (previously created in Microsoft Dynamics 365 BC), authenticates him based on his voice or face, and then gets orders' details, creates a new sales order, or deletes an existing order. <br/>
It can speak and understand 6 different languages, and can understand natural language.<br />
It can understand voice messages.<br />

## It consumes REST APIs of:
* Microsoft Dynamics 365 Business Central
  * API created by Microsoft and documented (https://docs.microsoft.com/en-us/dynamics-nav/fin-graph/)
  * To create sales orders, get sales orders, delete sales orders, and get customer details.
* Microsoft LUIS AI:
  * API created by Microsoft and documented (https://docs.microsoft.com/en-us/azure/cognitive-services/luis/what-is-luis)
  * To understand natural language by identifying the client's intention (StartOver, Help, CreateOrder, ViewOrders, etc.).
* Microsoft Speech To Text:
  * API created by Microsoft and documented (https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/)
  * To convert voice messages sent by the customer to text in order to understand them.
* Microsoft Text Translator:
  * API created by Microsoft and documented (https://docs.microsoft.com/en-us/azure/cognitive-services/translator/translator-info-overview)
  * To translate text from-to english based on client's preferred language.
* Microsoft Voice Verification:
  * API created by Microsoft and documented (https://docs.microsoft.com/en-us/azure/cognitive-services/speaker-recognition/home)
  * To authenticate customer based on his voice signiture.
  * Uses ML by adding an enrollment to profile after succesful entry.
  * Keeps pairing between BC profiles and verification profiles in Azure Table Storage.
* CloudConvert.com:
  * Documentation (https://cloudconvert.com/api/conversions)
  * To convert audio from ogg to wav to match Voice Verification audio requirements.
  
## It uses SDKs of:
* Microsoft Face API:
  * Created by Microsoft and documented (https://docs.microsoft.com/en-us/azure/cognitive-services/face/overview)
  * To authenticate the customer based on his picture enrollment.
  * Implements Machine Learning by adding an face enrollment after each successful entry.
 
 ## It also shows knowledge of:
 
* SOLID Design principles (Dependency Injection, Inversion of Control)
* Networking principles (Firewalls, Authentication, HTTP)
* Cloud Services (VMs, Azure)
* Version Control (project was maintained on Azure DevOps)
* Documentation


This project was developed in the period of December 20/2018 - January 20/2019 during a winter internship at Engineering Design & Manufacturing.
 
