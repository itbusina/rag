import ReactMarkdown from 'react-markdown'
import remarkGfm from 'remark-gfm'
import rehypeHighlight from 'rehype-highlight'

interface MarkdownContentProps {
  content: string
  className?: string
}

export function MarkdownContent({ content, className = '' }: MarkdownContentProps) {
  return (
    <div className={`markdown-wrapper prose prose-sm dark:prose-invert max-w-none ${className}`}>
      <ReactMarkdown
        remarkPlugins={[remarkGfm]}
        rehypePlugins={[rehypeHighlight]}
        components={{
          // Customize heading styles
          h1: ({ node, ...props }) => (
            <h1 className="text-2xl font-bold mt-6 mb-4 first:mt-0" {...props} />
          ),
          h2: ({ node, ...props }) => (
            <h2 className="text-xl font-bold mt-5 mb-3 first:mt-0" {...props} />
          ),
          h3: ({ node, ...props }) => (
            <h3 className="text-lg font-bold mt-4 mb-2 first:mt-0" {...props} />
          ),
          h4: ({ node, ...props }) => (
            <h4 className="text-base font-bold mt-3 mb-2 first:mt-0" {...props} />
          ),
          h5: ({ node, ...props }) => (
            <h5 className="text-sm font-bold mt-2 mb-1 first:mt-0" {...props} />
          ),
          h6: ({ node, ...props }) => (
            <h6 className="text-xs font-bold mt-2 mb-1 first:mt-0" {...props} />
          ),
          // Customize paragraph styles
          p: ({ node, ...props }) => (
            <p className="mb-3 leading-relaxed last:mb-0" {...props} />
          ),
          // Customize list styles
          ul: ({ node, ...props }) => (
            <ul className="list-disc list-outside mb-3 space-y-1 pl-5" {...props} />
          ),
          ol: ({ node, ...props }) => (
            <ol className="list-decimal list-outside mb-3 space-y-1 pl-5" {...props} />
          ),
          li: ({ node, ...props }) => (
            <li className="leading-relaxed" {...props} />
          ),
          // Customize code blocks
          code: ({ node, inline, className, children, ...props }: any) => {
            if (inline) {
              return (
                <code
                  className="bg-muted px-1.5 py-0.5 rounded text-sm font-mono border border-border"
                  {...props}
                >
                  {children}
                </code>
              )
            }
            return (
              <code
                className={`block text-sm font-mono ${className || ''}`}
                {...props}
              >
                {children}
              </code>
            )
          },
          pre: ({ node, ...props }) => (
            <pre className="bg-muted p-4 rounded-lg overflow-x-auto mb-3 border border-border" {...props} />
          ),
          // Customize blockquote styles
          blockquote: ({ node, ...props }) => (
            <blockquote
              className="border-l-4 border-primary pl-4 italic my-3 text-muted-foreground"
              {...props}
            />
          ),
          // Customize link styles
          a: ({ node, ...props }) => (
            <a
              className="text-primary hover:underline font-medium"
              target="_blank"
              rel="noopener noreferrer"
              {...props}
            />
          ),
          // Customize table styles
          table: ({ node, ...props }) => (
            <div className="overflow-x-auto mb-3">
              <table className="min-w-full border-collapse border border-border" {...props} />
            </div>
          ),
          thead: ({ node, ...props }) => (
            <thead className="bg-muted" {...props} />
          ),
          th: ({ node, ...props }) => (
            <th className="border border-border px-3 py-2 text-left font-bold text-sm" {...props} />
          ),
          td: ({ node, ...props }) => (
            <td className="border border-border px-3 py-2 text-sm" {...props} />
          ),
          tr: ({ node, ...props }) => (
            <tr className="hover:bg-muted/50" {...props} />
          ),
          // Customize horizontal rule
          hr: ({ node, ...props }) => (
            <hr className="my-4 border-border" {...props} />
          ),
          // Customize images
          img: ({ node, ...props }) => (
            // eslint-disable-next-line @next/next/no-img-element
            <img className="max-w-full h-auto rounded-lg my-3" alt="" {...props} />
          ),
          // Customize strong/bold text
          strong: ({ node, ...props }) => (
            <strong className="font-bold text-foreground" {...props} />
          ),
          // Customize emphasis/italic text
          em: ({ node, ...props }) => (
            <em className="italic" {...props} />
          ),
        }}
      >
        {content}
      </ReactMarkdown>
    </div>
  )
}

