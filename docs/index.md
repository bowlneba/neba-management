---
layout: default
title: Home
---

<div class="content-header">
    <h2>NEBA Management Documentation</h2>
    <p>Welcome to the technical documentation for the Northern Erie Bowling Association Management application. This documentation covers domain modeling, architecture, and development guides.</p>
</div>

<div class="feature-grid">
    <div class="feature-box">
        <h3><span class="feature-icon">📖</span> Ubiquitous Language <span class="status-badge status-ready">Ready</span></h3>
        <p>Domain terminology and definitions for consistent communication.</p>
        <ul class="feature-list">
            <li><a href="{{ '/ubiquitous-language/awards-and-titles' | relative_url }}">Awards & Titles</a></li>
            <li>Bowlers <span class="status-badge status-soon">Soon</span></li>
            <li>Members <span class="status-badge status-soon">Soon</span></li>
            <li>Tournaments <span class="status-badge status-soon">Soon</span></li>
        </ul>
    </div>

    <div class="feature-box">
        <h3><span class="feature-icon">🏗️</span> Architecture <span class="status-badge status-ready">Ready</span></h3>
        <p>Modular monolith with DDD tactical patterns.</p>
        <ul class="feature-list">
            <li><a href="{{ '/architecture/bounded-contexts' | relative_url }}">Bounded Contexts</a></li>
            <li><a href="{{ '/architecture/adr-001-ulid-shadow-keys' | relative_url }}">ADR-001: ULID Pattern</a></li>
            <li><a href="{{ '/architecture/adr-0050-opentelemetry-without-aspire-apphost' | relative_url }}">ADR-005: OpenTelemetry</a></li>
            <li>Domain Models <span class="status-badge status-soon">Soon</span></li>
            <li>CQRS Implementation <span class="status-badge status-soon">Soon</span></li>
        </ul>
    </div>

    <div class="feature-box">
        <h3><span class="feature-icon">📊</span> Observability <span class="status-badge status-ready">Ready</span></h3>
        <p>OpenTelemetry metrics, traces, and logs.</p>
        <ul class="feature-list">
            <li><a href="{{ '/observability/telemetry-reference' | relative_url }}">Telemetry Reference</a></li>
            <li>Monitoring Dashboards <span class="status-badge status-soon">Soon</span></li>
            <li>Alert Configuration <span class="status-badge status-soon">Soon</span></li>
        </ul>
    </div>

    <div class="feature-box">
        <h3><span class="feature-icon">⚙️</span> Admin Guides</h3>
        <p>Procedures for managing the NEBA application.</p>
        <ul class="feature-list">
            <li>Data Import</li>
            <li>User Management</li>
            <li>Tournament Setup</li>
            <li>System Maintenance</li>
        </ul>
    </div>

    <div class="feature-box">
        <h3><span class="feature-icon">🔌</span> API Reference</h3>
        <p>Complete API documentation and examples.</p>
        <ul class="feature-list">
            <li>Endpoints</li>
            <li>Authentication</li>
            <li>Request/Response</li>
            <li>Error Handling</li>
        </ul>
    </div>

    <div class="feature-box">
        <h3><span class="feature-icon">🎨</span> UI Components</h3>
        <p>Frontend design system and component library.</p>
        <ul class="feature-list">
            <li>Theming Guide</li>
            <li>Component Catalog</li>
            <li>Design Patterns</li>
            <li>Accessibility</li>
        </ul>
    </div>

    <div class="feature-box">
        <h3><span class="feature-icon">🗄️</span> Database</h3>
        <p>Database schema and migration guides.</p>
        <ul class="feature-list">
            <li>Schema Overview</li>
            <li>Migrations</li>
            <li>Data Models</li>
            <li>Queries</li>
        </ul>
    </div>
</div>
