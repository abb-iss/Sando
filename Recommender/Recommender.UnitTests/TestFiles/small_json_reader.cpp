#include <json/reader.h>
#include <json/value.h>
#include <utility>
#include <cstdio>
#include <cassert>
#include <cstring>
#include <iostream>
#include <stdexcept>

namespace Json {

static bool 
containsNewLine( Reader::Location begin, 
                 Reader::Location end )
{
   for ( ;begin < end; ++begin )
      if ( *begin == '\n'  ||  *begin == '\r' )
         return true;
   return false;
}

static std::string codePointToUTF8(unsigned int cp)
{
   std::string result;
   
   // based on description from http://en.wikipedia.org/wiki/UTF-8

   if (cp <= 0x7f) 
   {
      result.resize(1);
      result[0] = static_cast<char>(cp);
   } 
   else if (cp <= 0x7FF) 
   {
      result.resize(2);
      result[1] = static_cast<char>(0x80 | (0x3f & cp));
      result[0] = static_cast<char>(0xC0 | (0x1f & (cp >> 6)));
   } 
   else if (cp <= 0xFFFF) 
   {
      result.resize(3);
      result[2] = static_cast<char>(0x80 | (0x3f & cp));
      result[1] = 0x80 | static_cast<char>((0x3f & (cp >> 6)));
      result[0] = 0xE0 | static_cast<char>((0xf & (cp >> 12)));
   }
   else if (cp <= 0x10FFFF) 
   {
      result.resize(4);
      result[3] = static_cast<char>(0x80 | (0x3f & cp));
      result[2] = static_cast<char>(0x80 | (0x3f & (cp >> 6)));
      result[1] = static_cast<char>(0x80 | (0x3f & (cp >> 12)));
      result[0] = static_cast<char>(0xF0 | (0x7 & (cp >> 18)));
   }

   return result;
}


// Class Reader
// //////////////////////////////////////////////////////////////////

Reader::Reader()
{
}

bool
Reader::parse( const std::string &document, 
               Value &root,
               bool collectComments )
{
   document_ = document;
   const char *begin = document_.c_str();
   const char *end = begin + document_.length();
   return parse( begin, end, root, collectComments );
}

bool
Reader::parse( std::istream& sin,
               Value &root,
               bool collectComments )
{
   std::string doc;
   std::getline(sin, doc, (char)EOF);
   return parse( doc, root, collectComments );
}

} //end of Json namespace

