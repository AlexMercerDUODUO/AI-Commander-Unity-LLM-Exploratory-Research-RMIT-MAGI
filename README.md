# AI-Commander-Unity-LLM-Research-RMIT-MAGI
A practice-based research project from RMIT University on creating a low-cost LLM-Unity communication pipeline for indie game AI.
# AI-Commander-Unity-LLM-Exploratory-Research-RMIT-MAGI

A practice-based research project from RMIT University on creating a low-cost LLM-Unity communication pipeline for indie game AI.
# The AI Commander: An Exploratory Study of LLM-Unity Communication

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![RMIT University](https://img.shields.io/badge/RMIT-University-blue)](https://www.rmit.edu.au/)
[![MAGI S4 2025](https://img.shields.io/badge/MAGI_S4-2025-orange)](https://www.rmit.edu.au/study-with-us/levels-of-study/postgraduate-study/masters-by-coursework/master-of-animation-games-and-interactivity-mc219)

> A practice-based research project demonstrating a low-cost communication pipeline between Unity and Large Language Models for resource-constrained game developers.

---

### 🎓 Scholarly Video Essay (6分36秒)

[![Watch the video essay on YouTube](https://img.youtube.com/vi/tYeDCry3vtY/hqdefault.jpg)](https://www.youtube.com/watch?v=tYeDCry3vtY)

**[▶️ Click here to watch the full video essay on YouTube](https://www.youtube.com/watch?v=tYeDCry3vtY)**

---

### 📝 Abstract

This paper documents an exploratory study into a potential architectural approach: establishing a low-cost communication pipeline between the Unity game engine and an external LLM (DeepSeek). Through a practice-based research methodology involving functional prototyping and systematic observation, this study reports on the design and preliminary performance of a hierarchical model where the LLM provides high-level tactical commands while traditional state machines handle real-time execution.

---

### 🏗️ Core Architecture

The "AI Commander" system is built on a hierarchical, two-layer architecture that separates strategic reasoning from tactical execution. This approach, guided by the Command Pattern, allows an external LLM to act as a high-level "commander" without the heavy costs and latency of real-time control.

![Core Architecture Diagram](Images/architecture_diagram.png)

---

### 💡 Key Findings

1.  **Technical Viability:** The communication pipeline between Unity and the LLM is technically functional and robust.
2.  **Remarkable Cost-Effectiveness:** Over a 4-month testing period with 454 API calls, the total operational cost was only **¥0.67 CNY (~$0.58 USD)**, proving the economic feasibility for indie developers.
3.  **Emergent & Unscripted Behavior:** The LLM autonomously generated a "hold" command—a conservative tactic that was never explicitly programmed—demonstrating contextual reasoning beyond its instructions.

![Cost Dashboard](Images/cost_dashboard.png)

---

### 🛠️ Tech Stack

*   **Game Engine:** Unity
*   **Programming Language:** C#
*   **LLM Service:** DeepSeek API

---

### 🚀 Getting Started

The core logic for the AI Commander can be found in the `/Code` directory. The full research methodology and findings are detailed in the paper located in the `/Paper` directory.

---

### 📄 How to Cite This Work

If you find this research useful, please cite it as follows:

```bibtex
@misc{zhuang2025aicommaner,
  author       = {Zhuang, Haoduo (Alex)},
  title        = {The AI Commander: An Exploratory Study of LLM-Unity Communication for Resource-Constrained Game Developers in 2D Top-Down Shooters},
  year         = {2025},
  publisher    = {GitHub},
  journal      = {GitHub repository},
  howpublished = {\url{https://github.com/AlexMercerDUODUO/AI-Commander-Unity-LLM-Exploratory-Research-RMIT-MAGI}}
}
