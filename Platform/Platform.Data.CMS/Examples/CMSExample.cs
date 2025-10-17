using System;
using System.Collections.Generic;

namespace Platform.Data.CMS.Examples
{
    /// <summary>
    /// Example demonstrating how to use LinksBasedCMSStorage for CMS content management.
    /// This example shows how CMS systems like Orchard, Umbraco, or DNN could use
    /// LinksPlatform's Doublets as their data layer.
    ///
    /// Note: This is a conceptual example. To run it, you would need to:
    /// 1. Initialize an ILinks implementation (e.g., UnitedMemoryLinks with a file path)
    /// 2. Pass it to the LinksBasedCMSStorage constructor
    /// 3. Execute the example operations shown below
    /// </summary>
    public class CMSExample
    {
        public static void ConceptualRun()
        {
            Console.WriteLine("=== LinksPlatform CMS Data Layer Example ===\n");
            Console.WriteLine("This example demonstrates the CMS data layer concept.\n");

            // Example 1: Creating content items (like a blog post)
            Console.WriteLine("1. Create a blog post:");
            Console.WriteLine("   var blogPostId = cmsStorage.CreateContent(\"BlogPost\", new Dictionary<string, object>");
            Console.WriteLine("   {");
            Console.WriteLine("       { \"Title\", \"Introduction to LinksPlatform\" },");
            Console.WriteLine("       { \"Author\", \"John Doe\" },");
            Console.WriteLine("       { \"Content\", \"LinksPlatform is an associative data storage system...\" },");
            Console.WriteLine("       { \"PublishedDate\", \"2021-01-15\" },");
            Console.WriteLine("       { \"Status\", \"Published\" }");
            Console.WriteLine("   });\n");

            // Example 2: Creating another content item (a page)
            Console.WriteLine("2. Create a static page:");
            Console.WriteLine("   var pageId = cmsStorage.CreateContent(\"Page\", new Dictionary<string, object>");
            Console.WriteLine("   {");
            Console.WriteLine("       { \"Title\", \"About Us\" },");
            Console.WriteLine("       { \"Content\", \"We are building the future of data storage...\" },");
            Console.WriteLine("       { \"Slug\", \"about-us\" },");
            Console.WriteLine("       { \"Status\", \"Published\" }");
            Console.WriteLine("   });\n");

            // Example 3: Creating a media item
            Console.WriteLine("3. Create a media item:");
            Console.WriteLine("   var mediaId = cmsStorage.CreateContent(\"Media\", new Dictionary<string, object>");
            Console.WriteLine("   {");
            Console.WriteLine("       { \"FileName\", \"logo.png\" },");
            Console.WriteLine("       { \"FilePath\", \"/media/images/logo.png\" },");
            Console.WriteLine("       { \"MimeType\", \"image/png\" },");
            Console.WriteLine("       { \"Size\", \"12345\" }");
            Console.WriteLine("   });\n");

            // Example 4: Reading content
            Console.WriteLine("4. Reading blog post content:");
            Console.WriteLine("   var blogPostData = cmsStorage.GetContent(blogPostId);");
            Console.WriteLine("   // Returns dictionary with content properties\n");

            // Example 5: Updating content
            Console.WriteLine("5. Updating blog post status:");
            Console.WriteLine("   cmsStorage.UpdateContent(blogPostId, new Dictionary<string, object>");
            Console.WriteLine("   {");
            Console.WriteLine("       { \"Status\", \"Draft\" },");
            Console.WriteLine("       { \"LastModified\", \"2021-01-20\" }");
            Console.WriteLine("   });\n");

            // Example 6: Creating relationships between content items
            Console.WriteLine("6. Creating relationships:");
            Console.WriteLine("   var relationship1 = cmsStorage.CreateRelationship(blogPostId, mediaId, \"featured-image\");");
            Console.WriteLine("   var relationship2 = cmsStorage.CreateRelationship(pageId, blogPostId, \"related-content\");\n");

            // Example 7: Querying related content
            Console.WriteLine("7. Finding related content:");
            Console.WriteLine("   var relatedToPost = cmsStorage.GetRelatedContent(blogPostId, \"featured-image\");\n");

            // Example 8: Querying content by type
            Console.WriteLine("8. Querying all blog posts:");
            Console.WriteLine("   var allBlogPosts = cmsStorage.QueryContent(\"BlogPost\");\n");

            // Example 9: Querying with filters
            Console.WriteLine("9. Querying published blog posts:");
            Console.WriteLine("   var publishedPosts = cmsStorage.QueryContent(\"BlogPost\", new Dictionary<string, object>");
            Console.WriteLine("   {");
            Console.WriteLine("       { \"Status\", \"Published\" }");
            Console.WriteLine("   });\n");

            // Example 10: Deleting content
            Console.WriteLine("10. Deleting content:");
            Console.WriteLine("    var deleted = cmsStorage.DeleteContent(mediaId);\n");

            Console.WriteLine("=== Conceptual example completed ===");
            Console.WriteLine("\nKey Concepts:");
            Console.WriteLine("- Content items are stored as links with type markers");
            Console.WriteLine("- Properties are represented as nested link structures");
            Console.WriteLine("- Relationships create direct associations between content");
            Console.WriteLine("- All data is persisted in the associative link storage");
        }
    }
}
