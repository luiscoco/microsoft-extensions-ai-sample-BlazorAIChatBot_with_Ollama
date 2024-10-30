# How to create a Blazor Web App (.NET9) with an AI ChatBot and Ollama phi3:latest model

## 1. Donwload and Install AI Ollama in your laptop

https://ollama.com/download

![image](https://github.com/user-attachments/assets/b89a70a5-a5ff-4f81-83e4-ea352d7d1fd9)

To verify the Ollama installation run these commands:

For downloading the phi3 model:

```
ollama run phi3
```

For listing the models:

```
ollama list
```

![image](https://github.com/user-attachments/assets/caf096b3-2d59-4534-b943-6685e3f1f3ef)

Verify Ollama is running

```
curl http://localhost:11434
```

![image](https://github.com/user-attachments/assets/f54ed356-d5b5-4e97-8652-e5abee40df6b)

Send a request:

```
curl -X POST http://localhost:11434/v1/completions ^
-H "Content-Type: application/json" ^
-d "{ \"model\": \"phi3:latest\", \"prompt\": \"hello\" }"
```

![image](https://github.com/user-attachments/assets/fd1abcd5-967a-44f8-adc4-82ed4d784783)

## 2. Create a Blazor Web App (.NET 9)

We run Visual Studio 2022 Community Edition and we Create a new Project

![image](https://github.com/user-attachments/assets/05a493e4-b72c-4158-b9d9-0a99ac210d12)

We select the Blazor Web App project template

![image](https://github.com/user-attachments/assets/c5c5f456-432e-42c9-bdaf-1539abd4fdcf)

We input the project name and location

![image](https://github.com/user-attachments/assets/6f26be1e-e554-41fe-8ec4-c4b8d0706af8)

We select the .NET 9 framework and leave the other options with the default values, and we press the Create button

![image](https://github.com/user-attachments/assets/43e7f31a-9505-43f1-b6c7-92368cae8b1f)

We verify the project folders and files structure

![image](https://github.com/user-attachments/assets/f7add044-2822-48a6-814b-b1a796eebee2)

## 3. Load the Nuget Packages

![image](https://github.com/user-attachments/assets/57c9b3f5-aa6f-4127-adb7-5329be217e50)

## 4. Modify the middleware(Program.cs)

```csharp

```

## 5. Add the Chatbot




## 6. Modify the Home.razor component


## 7. Run the application a see the results



