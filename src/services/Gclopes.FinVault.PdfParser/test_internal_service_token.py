import os
import unittest
from unittest.mock import patch

import jwt

from main import create_internal_service_token


class InternalServiceTokenTests(unittest.TestCase):
    @patch.dict(
        os.environ,
        {
            "JWT_SECRET": "test-secret-with-at-least-thirty-two-bytes",
            "JWT_ISSUER": "test-issuer",
            "JWT_AUDIENCE": "test-audience",
        },
        clear=True,
    )
    def test_creates_short_lived_token_for_internal_service_calls(self):
        token = create_internal_service_token()

        claims = jwt.decode(
            token,
            "test-secret-with-at-least-thirty-two-bytes",
            algorithms=["HS256"],
            issuer="test-issuer",
            audience="test-audience",
        )

        self.assertEqual(claims["sub"], "finvault-pdf-parser")
        self.assertGreater(claims["exp"], claims["iat"])


if __name__ == "__main__":
    unittest.main()
