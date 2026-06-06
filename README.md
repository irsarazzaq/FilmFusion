# 🎬 FilmFusion: Next-Generation Cinematic Telemetry & Interactive Streaming Portal

## 📌 Project Overview
**FilmFusion** is an advanced, enterprise-level ASP.NET Core MVC (Model-View-Controller) web application engineered for high-performance movie data streaming, real-time user interactivity, and dynamic system telemetry. 

This portal provides users with a highly responsive, modern UI/UX web platform while seamlessly integrating complex state management, session-driven persistent architectures, asynchronous AJAX processing engines, and an automated secure SMTP alerting pipeline in the background.

---

## 👥 Authors & Core System Contributors
This project has been researched, architected, designed, and implemented as a collaborative engineering effort by:
* **Ayesha Yasmeen** 
* **Irsa Maryam** 

---

## 🚀 Core Sub-Systems & Detailed Features
<img width="1346" height="619" alt="image" src="https://github.com/user-attachments/assets/981483e7-52ec-4be1-ace6-a5550a832cae" />


### 1. Advanced Two-Step User Onboarding Pipeline
* **Step 1: Security & Identity Endpoint Gate:** Collects a unique email and password on the registration page. The backend automatically extracts the username handle directly from the email address to speed up initial user lookups.
<img width="1352" height="651" alt="image" src="https://github.com/user-attachments/assets/5404b24e-c7c7-45a3-9f28-cddf7f31044b" />
<img width="1358" height="624" alt="image" src="https://github.com/user-attachments/assets/696c7cbe-4025-41ce-b45a-4bf660e3141c" />


* **Step 2: Interactive Questionnaire & Preference Matrix:** Bypasses the user directly to an onboarding interest selection page where system telemetry rules compute default recommendations (e.g., mapping initial Sci-Fi preferences).
<img width="1345" height="639" alt="image" src="https://github.com/user-attachments/assets/f9091e60-3e35-420c-8201-c64646a2823a" />
<img width="1353" height="642" alt="image" src="https://github.com/user-attachments/assets/77111eee-6cd6-4a9a-9033-e81cdb14ca0f" />


### 2. Multi-Tier Authentication & Automated Telemetry Pipeline
* **Session Persistence Engine:** State maintenance across dashboard navigation is securely managed via standard `HttpContext.Session` architectures rather than temporary cryptographic client cookies.
* **Dual-Channel Automated SMTP Alerts:** Upon every successful login authentication, the backend triggers an automated security alert to the user's registered email via `smtp.gmail.com`.
* **Administrative Central Telemetry Logging:** Every operational login trace is dispatched as a real-time message payload directly to the Global Admin Master node (`admin@filmfusion.com`) for comprehensive system monitoring.
* **Hardcoded Administrative Bypass Console:** A custom login console handler (`admin` / `admin123`) bypasses standard framework restrictions to grant instant access to the internal infrastructure monitoring dashboards (`AdminConsole`).
<img width="1353" height="640" alt="image" src="https://github.com/user-attachments/assets/898f0232-a5ed-460a-bc19-4738e8f848f2" />
<img width="1343" height="651" alt="image" src="https://github.com/user-attachments/assets/926e38d5-46fd-4ece-8fb1-89367ca61889" />
<img width="1339" height="626" alt="image" src="https://github.com/user-attachments/assets/92e27f38-c8aa-42f0-b31b-8003ebacb961" />
<img width="356" height="288" alt="image" src="https://github.com/user-attachments/assets/8f228a2b-a3db-42c6-96ca-aed49fddff07" />
<img width="370" height="437" alt="image" src="https://github.com/user-attachments/assets/f11701dc-c65c-413d-8fec-1c9110dffac3" />



### 3. Dynamic Asynchronous User Interaction Engine
* **AJAX Preference Updates:** The `UpdateUserPreferences` route updates user-centric interest tags inside database records on-the-fly using jQuery/Fetch API payload interceptors without requiring a webpage reload.
* **Automated Robot Avatar Mapping Engine:** Programmatically connects with the external DiceBear Microservice API to generate dynamic vector robot avatars (`.svg`) based on each unique username string seed.
* **Dynamic Watch History Tracker:** Captures precise video playback progress percentages along with system timestamps (`DateTime.UtcNow`), appending them directly to the `WatchHistories` dataset.
* **Reactive Feedback Engine:** Links active UI star-rating controller inputs directly with the mutable context references of the database layout nodes to compile real-time ratings.
* **Raw SQL Structural Query Executions:** Optimizes data transactions by using direct string-interpolated database commands (`ExecuteSqlRawAsync`) for the Favorites and Watch Later modules instead of standard Entity Framework heavy loops.
<img width="1361" height="644" alt="image" src="https://github.com/user-attachments/assets/0d344b5d-b7be-4ed9-9159-acb1c251b71a" />
<img width="1354" height="536" alt="image" src="https://github.com/user-attachments/assets/50645b92-0a32-4772-9df0-f3e497dd5d3f" />
<img width="1353" height="476" alt="image" src="https://github.com/user-attachments/assets/21593145-506d-4425-8160-3e3d0ea30417" />
<img width="1354" height="423" alt="image" src="https://github.com/user-attachments/assets/cf3fe534-c9c4-4b6f-8138-46a812d9fb8d" />
<img width="1312" height="428" alt="image" src="https://github.com/user-attachments/assets/bc0bd751-d4d5-439b-94a6-46edbf95ed60" />
<img width="1357" height="452" alt="image" src="https://github.com/user-attachments/assets/b51ddd53-ab13-4536-a719-379f0bbc229e" />

---

## 🛠️ Technological Infrastructure & Ecosystem

* **Core Framework:** ASP.NET Core MVC (Target Runtime Framework v6.0 / v8.0 Architecture Components).
<img width="387" height="116" alt="image" src="https://github.com/user-attachments/assets/f3967fd7-da39-46e1-b457-6b51a10e94d4" />

* **Database Management System (DBMS):** Relational Engine powered by Microsoft SQL Server / PostgreSQL Infrastructure.
<img width="407" height="546" alt="image" src="https://github.com/user-attachments/assets/f28d3dab-09f3-4956-85dd-2ba1c062d79c" />

* **Object-Relational Mapping (ORM):** High-Performance Entity Framework Core via structured Object-Context mapping layer (`ApplicationDbContext.cs`).
* **Frontend Design Stack:** Compiled using utility-first Tailwind CSS configurations, modern HTML5 layouts, JavaScript Fetch API/AJAX mechanics, and FontAwesome Vector icon sets.
* **Communication Pipeline:** Industrial Mail Transfer Engine via specialized network components (`System.Net.Mail` & `System.Net.NetworkInformation`).

---

## 🗺️ High-Level System Workflow Architecture
