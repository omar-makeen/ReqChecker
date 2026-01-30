# Placeholder signature file for bundled profile
# In production, this will contain the HMAC-SHA256 signature of the normalized profile JSON
# Signature format: Base64 encoded HMAC-SHA256 hash
# 
# To generate a real signature:
# 1. Normalize the JSON (remove all whitespace)
# 2. Compute HMAC-SHA256 using the embedded key
# 3. Encode the result as Base64
#
# Example: <base64-encoded-signature>
