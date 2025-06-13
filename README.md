# 🧰 MDPHelperSuite

A complete system designed to support **Hardware/Software Testing and Validation** for the **Multidisciplinary Design Project (MDP)** at NTU.

This repository consolidates all components of the FYP into a single, public codebase for ease of access and demonstration.

---

## 📦 Project Structure

- **/SC4051-distributed-booking-system/**
  - `MDPHelper/` # Desktop GUI application for STM32 hardware/software testing
  - `server/` # Backend API server for data management
  - `UpdaterApp/` # Console tool to facilitate app updates

---

## 🧠 About the Project

This Final Year Project (FYP) aims to assist NTU CCDS students who are taking MDP by providing a **test & validation ecosystem** that streamlines development with the STM32 board.  
It reduces time spent troubleshooting hardware/firmware issues, allowing teams to focus more on innovation and functionality.

---

## 📁 Components

### 🔹 [MDPHelper](./MDPHelper/)
A **.NET MAUI desktop application** that offers:
- Automated hardware/software validation
- Serial communication interface
- Preloaded firmware upload
- Real-time test result logging

### 🔹 [Server](./Server/)
A **Flask RESTful API** to:
- Store test metadata and firmware binaries
- Support file versioning
- Interface with the MDPHelper desktop client

### 🔹 [UpdaterApp](./UpdaterApp/)
A simple **console tool** to:
- Check for available updates from the server
- Automatically download and apply updates to MDPHelper

---

## 🧑‍💻 Tech Stack

![C#](https://img.shields.io/badge/-CSharp-05122A?style=flat&logo=c-sharp&logoColor=239120)
![Python](https://img.shields.io/badge/-Python-05122A?style=flat&logo=python)
![Flask](https://img.shields.io/badge/-Flask-05122A?style=flat&logo=flask)
![.NET MAUI](https://img.shields.io/badge/-.NET%20MAUI-05122A?style=flat&logo=dotnet&logoColor=512BD4)
![SQLite](https://img.shields.io/badge/-SQLite-05122A?style=flat&logo=sqlite&logoColor=003B57)
![Git](https://img.shields.io/badge/-Git-05122A?style=flat&logo=git)
![GitHub](https://img.shields.io/badge/-GitHub-05122A?style=flat&logo=github)

---

## 📄 Documentation

For full details, diagrams, and implementation strategy, refer to:  
📘 [Final Project Report (PDF)](./docs/fyp_report.pdf) 
📽️ [Final Year Project Slide (PDF)](./docs/fyp_presentation_slide.pdf) 

---

## 🚀 Getting Started

Each subproject contains its own `README.md` with setup and run instructions.

To get started locally:

```bash
git clone https://github.com/Iqoyz/MDPHelperSuite.git
cd MDPHelperSuite/MDPHelper   # or Server, UpdaterApp
