window.monacoInterop = {
    initializeEditor: function (containerId, initialValue, dotnetHelper) {
        const languageId = "SimpleRegex";
        require.config({ paths: { vs: 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.52.0/min/vs' } });
        require(['vs/editor/editor.main'], function () {

            // Register your custom language
            monaco.languages.register({ id: languageId });

            // Define language configuration
            monaco.languages.setLanguageConfiguration(
                languageId,
                {
                    comments: {
                        lineComment: '//',
                    },
                    brackets: [['(', ')'],],
                    autoClosingPairs: [
                        { open: '(', close: ')' },
                        { open: '"', close: '"' },
                    ],
                    surroundingPairs: [
                        { open: '(', close: ')' },
                        { open: '"', close: '"' },
                    ],
                });

            // Define tokenization rules
            monaco.languages.setMonarchTokensProvider(
                languageId,
                {
                    ignoreCase: true,
                    quantifiers: ['maybe', 'maybeMany', 'many', 'lazy', 'exactly', 'atLeast', 'between'],
                    special: ['any', 'start', 'end'],
                    escaped: ['ws', 'whitespace', 'digit', 'word', 'boundary', 'newline', 'nl', 'cr', 'tab', 'null', 'quote'],
                    binary: ['or'],
                    operators: /[+]/,
                    tokenizer: {
                        root: [
                            [/[a-z_$][\w$]*/, {
                                cases: {
                                    '@quantifiers': 'quantifier',
                                    '@special': 'special',
                                    '@escaped': 'special',
                                    '@binary': 'binary',
                                    '@default': 'identifier',
                                }
                            }],
                            [/[()]/, '@brackets'],
                            [/\d+/, 'number'],
                            [/@operators/, 'binary'],
                            [/".*?"/, 'string'],
                            [/\/\/.*/, 'comment'],
                        ],
                    },
                });

            // Define custom themes
            monaco.editor.defineTheme('SimpleRegex', {
                base: 'vs-dark',
                inherit: true, // Inherit from the base theme
                rules: [
                    { token: 'quantifier', foreground: 'd7b96e' },   // Yellow quantifiers.
                    { token: 'special', foreground: '8cbed2' },      // Blue special/escaped characters.
                    { token: 'binary', foreground: 'b4b4b4' },       // Gray "or" and "+".
                ],
                colors: {} // For some reason this is needed.
            });

            // Create the editor instance
            const editor = monaco.editor.create(document.getElementById(containerId), {
                value: initialValue,
                language: languageId,
                theme: 'SimpleRegex',
                lineNumbers: 'on',
                glyphMargin: false,
                folding: true,
                scrollbar: {
                    vertical: 'auto',
                    horizontal: 'auto',
                },
                minimap: { enabled: false },
            });

            // Set up the event listener for content changes
            editor.onDidChangeModelContent(function () {
                const currentValue = editor.getValue();
                // Call a Blazor method to notify about the value change
                dotnetHelper.invokeMethodAsync('OnEditorValueChanged', currentValue);
            });
        });
    },
};
