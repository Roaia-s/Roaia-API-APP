# Roaia API

This repository contains the API for the Roaia project, which aims to assist visually impaired individuals by utilizing smart glasses. The glasses provide features like text reading, object detection, face recognition, weather updates, and emergency notifications.

## Features

- **Text Reading**: Converts text into speech for the user.
- **Object Detection**: Identifies objects in the user's surroundings.
- **Face Recognition**: Recognizes pre-registered faces.
- **Weather Updates**: Provides current weather information.
- **Emergency Notifications**: Sends SOS alerts to a connected mobile application.

### Smart Glasses

- `POST /api/glasses/location`: Send current location
- `POST /api/glasses/notification`: Send notification to the app

### Notifications

- `GET /api/notifications`: Retrieve notifications
- `POST /api/notifications`: Create a notification

## Installation

1. Clone the repository:
    ```sh
    git clone https://github.com/Roaia-s/Roaia-API-APP.git
    ```
2. Navigate to the project directory:
    ```sh
    cd Roaia-API-APP
    ```
3. Install the dependencies:
    ```sh
    dotnet restore
    ```
4. Run the application:
    ```sh
    dotnet run
    ```

## Usage

After running the application, the API will be available at `http://localhost:5000`. You can use tools like Postman to interact with the API endpoints.

## Contributing

Contributions are welcome! Please fork this repository and submit a pull request for any enhancements or bug fixes.

## License

This project is licensed under the MIT License.
