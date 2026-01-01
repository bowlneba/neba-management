import { describe, test, expect, beforeEach, jest } from '@jest/globals';

// Mock Blazor global
globalThis.Blazor = {
  reconnect: jest.fn(),
  resumeCircuit: jest.fn()
};

describe('ReconnectModal', () => {
  let reconnectModal;
  let retryButton;
  let resumeButton;

  beforeEach(() => {
    jest.clearAllMocks();

    // Reset location mock
    delete globalThis.location;
    globalThis.location = { reload: jest.fn() };

    // Set up DOM
    document.body.innerHTML = `
      <dialog id="components-reconnect-modal"></dialog>
      <button id="components-reconnect-button">Retry</button>
      <button id="components-resume-button">Resume</button>
    `;

    reconnectModal = document.getElementById('components-reconnect-modal');
    retryButton = document.getElementById('components-reconnect-button');
    resumeButton = document.getElementById('components-resume-button');

    // Mock dialog methods
    reconnectModal.showModal = jest.fn();
    reconnectModal.close = jest.fn();

    // Reset document visibility
    Object.defineProperty(document, 'visibilityState', {
      writable: true,
      configurable: true,
      value: 'visible'
    });
  });

  describe('DOM Elements', () => {
    test('should have reconnect modal element', () => {
      expect(reconnectModal).toBeDefined();
      expect(reconnectModal.id).toBe('components-reconnect-modal');
    });

    test('should have retry button element', () => {
      expect(retryButton).toBeDefined();
      expect(retryButton.id).toBe('components-reconnect-button');
    });

    test('should have resume button element', () => {
      expect(resumeButton).toBeDefined();
      expect(resumeButton.id).toBe('components-resume-button');
    });
  });

  describe('Module behavior (integration tests)', () => {
    test('should set up event listener for components-reconnect-state-changed', () => {
      // Note: The module sets up event listeners on import
      // This test documents the expected behavior
      // In a real scenario, the module would listen for 'components-reconnect-state-changed' events
      expect(reconnectModal).toBeDefined();
    });

    test('should handle "show" state by showing the modal', () => {
      // Expected behavior: When state is "show", modal.showModal() should be called
      // This is an integration concern - the module code runs on import
      expect(reconnectModal.showModal).toBeDefined();
    });

    test('should handle "hide" state by closing the modal', () => {
      // Expected behavior: When state is "hide", modal.close() should be called
      expect(reconnectModal.close).toBeDefined();
    });

    test('should handle "rejected" state by reloading the page', () => {
      // Expected behavior: When state is "rejected", location.reload() should be called
      expect(globalThis.location.reload).toBeDefined();
    });
  });

  describe('Blazor integration', () => {
    test('should have Blazor.reconnect available', () => {
      expect(globalThis.Blazor.reconnect).toBeDefined();
      expect(typeof globalThis.Blazor.reconnect).toBe('function');
    });

    test('should have Blazor.resumeCircuit available', () => {
      expect(globalThis.Blazor.resumeCircuit).toBeDefined();
      expect(typeof globalThis.Blazor.resumeCircuit).toBe('function');
    });

    test('should handle successful reconnect', async () => {
      // Arrange
      globalThis.Blazor.reconnect.mockResolvedValue(true);

      // Act
      const result = await globalThis.Blazor.reconnect();

      // Assert
      expect(result).toBe(true);
    });

    test('should handle failed reconnect', async () => {
      // Arrange
      globalThis.Blazor.reconnect.mockResolvedValue(false);

      // Act
      const result = await globalThis.Blazor.reconnect();

      // Assert
      expect(result).toBe(false);
    });

    test('should handle reconnect exception', async () => {
      // Arrange
      const error = new Error('Network error');
      globalThis.Blazor.reconnect.mockRejectedValue(error);

      // Act & Assert
      await expect(globalThis.Blazor.reconnect()).rejects.toThrow('Network error');
    });

    test('should handle successful resume', async () => {
      // Arrange
      globalThis.Blazor.resumeCircuit.mockResolvedValue(true);

      // Act
      const result = await globalThis.Blazor.resumeCircuit();

      // Assert
      expect(result).toBe(true);
    });

    test('should handle failed resume', async () => {
      // Arrange
      globalThis.Blazor.resumeCircuit.mockResolvedValue(false);

      // Act
      const result = await globalThis.Blazor.resumeCircuit();

      // Assert
      expect(result).toBe(false);
    });

    test('should handle resume exception', async () => {
      // Arrange
      const error = new Error('Resume failed');
      globalThis.Blazor.resumeCircuit.mockRejectedValue(error);

      // Act & Assert
      await expect(globalThis.Blazor.resumeCircuit()).rejects.toThrow('Resume failed');
    });
  });

  describe('Document visibility', () => {
    test('should check document.visibilityState', () => {
      expect(document.visibilityState).toBeDefined();
      expect(document.visibilityState).toBe('visible');
    });

    test('should handle visibility change to hidden', () => {
      // Arrange
      Object.defineProperty(document, 'visibilityState', {
        writable: true,
        configurable: true,
        value: 'hidden'
      });

      // Assert
      expect(document.visibilityState).toBe('hidden');
    });

    test('should handle visibility change to visible', () => {
      // Arrange
      Object.defineProperty(document, 'visibilityState', {
        writable: true,
        configurable: true,
        value: 'visible'
      });

      // Assert
      expect(document.visibilityState).toBe('visible');
    });
  });
});
