# NEBA Documentation Site

This directory contains the source for the NEBA Management documentation site, deployed via GitHub Pages.

## Structure

```
docs/
├── _config.yml              # Jekyll configuration
├── _layouts/                # Page layouts
│   └── default.html         # Main layout with sidebar navigation
├── _ubiquitous_language/    # Ubiquitous language collection
│   └── awards-and-titles.md # Awards & Titles domain terminology
├── assets/
│   └── css/
│       ├── neba_theme.css   # Auto-synced by GitHub Actions (gitignored)
│       └── docs.css         # Documentation-specific styles
├── .gitignore               # Ignores build artifacts and neba_theme.css
├── Gemfile                  # Ruby dependencies for local development
├── index.md                 # Landing page
└── README.md                # This file
```

## Local Development

### Quick Start (Recommended)

From the `docs/` directory, run:

```bash
./serve.sh
```

This script will:
1. Sync the theme file from the main app
2. Install dependencies
3. Start the Jekyll server

Then visit `http://localhost:5300/neba-management/` in your browser.

### Manual Setup

If you prefer to run commands manually:

1. **First time only:** Install Jekyll and bundler:
   ```bash
   gem install bundler jekyll
   ```

2. **Copy the theme file:**
   ```bash
   cp src/frontend/Neba.Web.Server/wwwroot/neba_theme.css docs/assets/css/neba_theme.css
   ```

3. **Install dependencies:**
   ```bash
   cd docs
   bundle install
   ```

4. **Run Jekyll:**
   ```bash
   bundle exec jekyll serve
   ```

5. Visit `http://localhost:5300/neba-management/` in your browser

## GitHub Pages Deployment

The site is automatically deployed to GitHub Pages when changes are pushed to the `main` branch in the `docs/` directory.

**Workflow:** `.github/workflows/github-pages.yml`

### Enabling GitHub Pages

1. Go to your repository settings on GitHub
2. Navigate to **Pages** section
3. Under "Build and deployment":
   - Source: **GitHub Actions**
4. The workflow will automatically deploy on the next push to main

## Theme

The site uses the **exact same theme** as the NEBA application to ensure consistency:

- **Primary Color:** NEBA Blue (#052767)
- **Accent Color:** NEBA Blue 700 (#3E4FD9)
- **Layout:** Sidebar navigation (Option 3 from mockups)
- **Responsive:** Mobile-friendly with collapsing sidebar

### Theme Architecture

The documentation site uses a two-CSS-file approach:

1. **`neba_theme.css`** - Automatically synced from `src/frontend/Neba.Web.Server/wwwroot/neba_theme.css`
   - Contains all NEBA color variables, component styles, and design tokens
   - **Synced automatically** by GitHub Actions during deployment
   - This file is in `.gitignore` - do NOT commit it manually

2. **`docs.css`** - Documentation-specific styles only
   - Sidebar navigation layout
   - Documentation page structure
   - Content formatting specific to docs
   - Safe to edit for docs-specific styling

### How Theme Syncing Works

The theme is **automatically synced** during GitHub Pages deployment:

1. You edit `src/frontend/Neba.Web.Server/wwwroot/neba_theme.css` in the main app
2. Commit and push to `main` branch
3. GitHub Actions workflow automatically copies it to `docs/assets/css/` during build
4. No manual syncing needed!

**For local development:** If you need the theme file locally, run:

```bash
cp src/frontend/Neba.Web.Server/wwwroot/neba_theme.css docs/assets/css/neba_theme.css
```

The file is gitignored, so it won't be committed.

## Adding Content

### Adding a New Page

1. Create a new `.md` file in the appropriate directory
2. Add front matter:
   ```yaml
   ---
   layout: default
   title: Page Title
   ---
   ```
3. Write content in Markdown
4. Update navigation in `_layouts/default.html` if needed

### Adding to Ubiquitous Language

1. Create a new file in `_ubiquitous_language/` directory
2. Follow the structure of `history.md`
3. The file will be automatically available at `/ubiquitous-language/filename/`

## Current Status

### ✅ Complete
- [x] Jekyll configuration
- [x] Sidebar navigation layout
- [x] NEBA-themed styles
- [x] Landing page
- [x] Ubiquitous language structure for History domain
- [x] GitHub Pages deployment workflow

### 🚧 In Progress
- [ ] Filling out History domain term definitions
- [ ] Updating code XML comments to match ubiquitous language

### 📋 Planned
- [ ] Members domain ubiquitous language
- [ ] Tournaments domain ubiquitous language
- [ ] Domain models documentation
- [ ] Admin procedures guides
- [ ] API reference
- [ ] UI components documentation
- [ ] Database schema documentation

## Contributing

When adding or updating documentation:

1. Ensure consistency with the ubiquitous language
2. Use clear, concise definitions
3. Link to relevant code files
4. Include examples where helpful
5. Keep the sidebar navigation updated
6. Test locally before pushing

## Questions?

See the main project README or contact the development team.
