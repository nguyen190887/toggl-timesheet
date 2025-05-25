/// <reference types="vite/client" />
import React, { useState, useEffect } from 'react';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import CircularProgress from '@mui/material/CircularProgress';

const App: React.FC = () => {
  const [startDate, setStartDate] = useState<string>('');
  const [endDate, setEndDate] = useState<string>('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const today = new Date();
    const closestSunday = new Date(today);
    closestSunday.setDate(today.getDate() - today.getDay()); // Set to the closest Sunday in the past

    const closestSundayISO = closestSunday.toISOString().split('T')[0];
    setEndDate(closestSundayISO);

    const startDate = new Date(closestSunday);
    startDate.setDate(closestSunday.getDate() - 6); // 6 days before the closest Sunday
    const startDateISO = startDate.toISOString().split('T')[0];
    setStartDate(startDateISO);
  }, []);

  const handleGetTimeReport = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const baseUrl = import.meta.env.VITE_API_BASE_URL;
      const response = await fetch(
        `${baseUrl}/download-time-report?startDate=${startDate}&endDate=${endDate}`,
        { method: 'GET' }
      );

      if (!response.ok) {
        throw new Error(`Failed to fetch report: ${response.statusText}`);
      }

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `timesheet-${startDate}-to-${endDate}.csv`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to download report');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        gap: 2,
        maxWidth: 400,
        margin: 'auto',
        padding: 2,
        borderRadius: 2,
        boxShadow: 3,
      }}
    >
      <Typography variant="h4" component="h1" gutterBottom>
        Timesheet Report App
      </Typography>
      <TextField
        label="Start Date"
        type="date"
        value={startDate}
        onChange={(e) => setStartDate(e.target.value)}
        InputLabelProps={{ shrink: true }}
        disabled={isLoading}
      />
      <TextField
        label="End Date"
        type="date"
        value={endDate}
        onChange={(e) => setEndDate(e.target.value)}
        InputLabelProps={{ shrink: true }}
        disabled={isLoading}
      />
      {error && (
        <Typography color="error" variant="body2">
          {error}
        </Typography>
      )}
      <Button
        variant="contained"
        color="primary"
        onClick={handleGetTimeReport}
        disabled={isLoading}
        sx={{ mt: 2 }}
      >
        {isLoading ? (
          <>
            <CircularProgress size={24} color="inherit" sx={{ mr: 1 }} />
            Generating Report...
          </>
        ) : (
          'Generate Report'
        )}
      </Button>
    </Box>
  );
};

export default App;
